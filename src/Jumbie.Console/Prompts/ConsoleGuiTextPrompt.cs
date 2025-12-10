using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using ConsoleGUI.Api;
using ConsoleGUI.Common;
using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;
using ConsoleGUI.Utils;
using Spectre.Console;
using ConsoleGuiSize = ConsoleGUI.Space.Size;
using Jumbie.Console;

namespace Jumbie.Console.Prompts
{
    public class ConsoleGuiTextPrompt<T> : Control, IInputListener
    {
        private readonly string _prompt;
        private readonly StringComparer? _comparer;
        private readonly BufferConsole _bufferConsole;
        private readonly ConsoleGuiAnsiConsole _ansiConsole;

        private string _input = string.Empty;
        private int _caretPosition = 0;
        private string? _validationError = null;

        public event EventHandler<T>? Committed;

        public Style? PromptStyle { get; set; }
        public List<T> Choices { get; } = new List<T>();
        public CultureInfo? Culture { get; set; }
        public string InvalidChoiceMessage { get; set; } = "[red]Please select one of the available options[/]";
        public bool IsSecret { get; set; }
        public char? Mask { get; set; } = '*';
        public string ValidationErrorMessage { get; set; } = "[red]Invalid input[/]";
        public bool ShowChoices { get; set; } = true;
        public bool ShowDefaultValue { get; set; } = true;
        public bool AllowEmpty { get; set; }
        public Func<T, string>? Converter { get; set; }
        public Func<T, ValidationResult>? Validator { get; set; }
        public Style? DefaultValueStyle { get; set; }
        public Style? ChoicesStyle { get; set; }
        internal DefaultPromptValue<T>? DefaultValue { get; set; } // Simplified: public setter in adapting? internal matches source

        // Wrapper for internal DefaultValue
        public void SetDefaultValue(T value)
        {
            DefaultValue = new DefaultPromptValue<T>(value);
        }

        public ConsoleGuiTextPrompt(string prompt, StringComparer? comparer = null)
        {
            _prompt = prompt ?? throw new ArgumentNullException(nameof(prompt));
            _comparer = comparer;
            _bufferConsole = new BufferConsole();
            _ansiConsole = new ConsoleGuiAnsiConsole(_bufferConsole);
        }

        public override Cell this[Position position]
        {
            get
            {
                if (_bufferConsole.Buffer == null) return new Cell(Character.Empty);
                if (position.X < 0 || position.X >= Size.Width || position.Y < 0 || position.Y >= Size.Height) return new Cell(Character.Empty);
                return _bufferConsole.Buffer[position.X, position.Y];
            }
        }

        protected override void Initialize()
        {
            var targetSize = MaxSize;
            if (targetSize.Width > 1000) targetSize = new ConsoleGuiSize(1000, targetSize.Height);
            if (targetSize.Height > 1000) targetSize = new ConsoleGuiSize(targetSize.Width, 1000);

            Resize(targetSize);
            _bufferConsole.Resize(Size);

            Render();
        }

        private void Render()
        {
            if (Size.Width <= 0 || Size.Height <= 0) return;

            _ansiConsole.Clear(true);

            // 1. Build Prompt Markup
            var builder = new StringBuilder();
            builder.Append(_prompt.TrimEnd());

            var appendSuffix = false;
            var converter = Converter ?? TypeConverterHelper.ConvertToString;

            if (ShowChoices && Choices.Count > 0)
            {
                appendSuffix = true;
                var choices = string.Join("/", Choices.Select(choice => converter(choice)));
                var choicesStyle = ChoicesStyle?.ToMarkup() ?? "blue";
                builder.AppendFormat(CultureInfo.InvariantCulture, " [{0}][[{1}]][/]", choicesStyle, choices);
            }

            if (ShowDefaultValue && DefaultValue != null)
            {
                appendSuffix = true;
                var defaultValueStyle = DefaultValueStyle?.ToMarkup() ?? "green";
                var defaultValue = converter(DefaultValue.Value.Value);
                // Note: Masking default value if secret? TextPrompt source does: IsSecret ? defaultValue.Mask(Mask) : defaultValue
                // We don't have Mask extension here easily unless we copy it. 
                // Simple masking:
                var displayDefault = IsSecret && Mask.HasValue ? new string(Mask.Value, defaultValue.Length) : defaultValue;
                
                builder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    " [{0}]({1})[/]",
                    defaultValueStyle,
                    displayDefault);
            }

            var markup = builder.ToString().Trim();
            if (appendSuffix)
            {
                markup += ":";
            }
            
            _ansiConsole.Markup(markup + " ");

            // 2. Render Input
            // We need to render the input. If it's secret, mask it.
            string displayInput = _input;
            if (IsSecret && Mask.HasValue)
            {
                displayInput = new string(Mask.Value, _input.Length);
            }
            
            // We want to highlight the cursor or just render it? 
            // Spectre's AnsiConsole doesn't really have a "cursor" visual in the buffer unless we simulate it or relying on terminal cursor.
            // ConsoleGUI relies on cells. 
            // We can simulate cursor by reversing colors at caret position if we want, 
            // OR we can just rely on ConsoleGUI's native cursor handling if we were a TextBox, 
            // but here we are rendering via Spectre.
            // Let's just write the text.
            
            // Note: We need to escape markup in input if not secret?
            // Actually, Write() in Spectre writes plain text, Markup() parses.
            // We use Write() for input.
            _ansiConsole.Write(displayInput);

            // 3. Render Error (if any)
            if (_validationError != null)
            {
                _ansiConsole.WriteLine();
                _ansiConsole.MarkupLine(_validationError);
            }
            
            Redraw();
        }

        void IInputListener.OnInput(InputEvent inputEvent)
        {
            // Standard text editing logic
            bool handled = false;
            string? newInput = null;

            switch (inputEvent.Key.Key)
            {
                case ConsoleKey.LeftArrow:
                    _caretPosition = Math.Max(0, _caretPosition - 1);
                    handled = true;
                    break;
                case ConsoleKey.RightArrow:
                    _caretPosition = Math.Min(_input.Length, _caretPosition + 1);
                    handled = true;
                    break;
                case ConsoleKey.Home:
                    _caretPosition = 0;
                    handled = true;
                    break;
                case ConsoleKey.End:
                    _caretPosition = _input.Length;
                    handled = true;
                    break;
                case ConsoleKey.Backspace:
                    if (_caretPosition > 0)
                    {
                        newInput = _input.Remove(_caretPosition - 1, 1);
                        _caretPosition--;
                        handled = true;
                    }
                    break;
                case ConsoleKey.Delete:
                    if (_caretPosition < _input.Length)
                    {
                        newInput = _input.Remove(_caretPosition, 1);
                        handled = true;
                    }
                    break;
                case ConsoleKey.Enter:
                    AttemptCommit();
                    handled = true;
                    break;
                default:
                    if (!char.IsControl(inputEvent.Key.KeyChar))
                    {
                        newInput = _input.Insert(_caretPosition, inputEvent.Key.KeyChar.ToString());
                        _caretPosition++;
                        handled = true;
                    }
                    break;
            }

            if (newInput != null)
            {
                _input = newInput;
            }

            if (handled)
            {
                inputEvent.Handled = true;
                // Re-render
                Render();
            }
        }

        private void AttemptCommit()
        {
             // Logic adapted from TextPrompt.ShowAsync loop
             
             // 1. Empty check
             if (string.IsNullOrWhiteSpace(_input))
             {
                 if (DefaultValue != null)
                 {
                     // Commit default
                     Committed?.Invoke(this, DefaultValue.Value.Value);
                     return;
                 }
                 
                 if (AllowEmpty)
                 {
                     // Convert empty string to T?
                     // If T is string, return empty.
                     // If T is nullable, null?
                     // Try convert empty string.
                     if (TypeConverterHelper.TryConvertFromStringWithCulture<T>(_input, Culture, out var emptyResult))
                     {
                         Committed?.Invoke(this, emptyResult!);
                         return;
                     }
                 }
                 
                 // If not allowed empty and no default, just ignore or show error?
                 // TextPrompt continues loop.
                 return; 
             }

             // 2. Choices check
             var converter = Converter ?? TypeConverterHelper.ConvertToString;
             // Re-create map (inefficient but safe)
             var choiceMap = Choices.ToDictionary(choice => converter(choice), choice => choice, _comparer);

             T? result;
             if (Choices.Count > 0)
             {
                 if (choiceMap.TryGetValue(_input, out result) && result != null)
                 {
                     // Valid choice
                 }
                 else
                 {
                     _validationError = InvalidChoiceMessage;
                     Render();
                     return;
                 }
             }
             else
             {
                 // 3. Conversion check
                 if (!TypeConverterHelper.TryConvertFromStringWithCulture<T>(_input, Culture, out result) || result == null)
                 {
                     _validationError = ValidationErrorMessage;
                     Render();
                     return;
                 }
             }

             // 4. Custom Validator
             if (Validator != null)
             {
                 var validationResult = Validator(result);
                 if (!validationResult.Successful)
                 {
                     _validationError = validationResult.Message ?? ValidationErrorMessage;
                     Render();
                     return;
                 }
             }

             // Success
             _validationError = null;
             Committed?.Invoke(this, result);
        }
    }
    
    // Minimal helper for internal DefaultPromptValue
    internal struct DefaultPromptValue<T>
    {
        public T Value { get; }
        public DefaultPromptValue(T value) => Value = value;
    }
}
