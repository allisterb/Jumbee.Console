using System;
using System.Threading;
using ConsoleGUI.Common;
using ConsoleGUI.Data;
using ConsoleGUI.Space;
using Spectre.Console;
using ConsoleGuiSize = ConsoleGUI.Space.Size;
using Jumbie.Console;

namespace Jumbie.Console.Controls
{
    public class ConsoleGuiSpinner : Control, IDisposable
    {
        private readonly object _lock = new object();
        private readonly BufferConsole _bufferConsole;
        private readonly ConsoleGuiAnsiConsole _ansiConsole;
        
        private Spinner _spinner = Spinner.Known.Default;
        private Style _style = Style.Plain;
        private string _text = string.Empty;
        
        private int _frameIndex;
        private DateTime _lastUpdate;
        private TimeSpan _accumulated;
        
        private bool _isRunning;

        public ConsoleGuiSpinner()
        {
            _bufferConsole = new BufferConsole();
            _ansiConsole = new ConsoleGuiAnsiConsole(_bufferConsole);
        }

        public Spinner Spinner 
        { 
            get => _spinner; 
            set 
            {
                lock(_lock) _spinner = value ?? Spinner.Known.Default; 
            }
        }
        
        public Style Style
        {
            get => _style;
            set
            {
                lock(_lock) _style = value ?? Style.Plain;
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                lock(_lock) 
                {
                    _text = value;
                    Render();
                }
            }
        }

        public void Start()
        {
            lock(_lock)
            {
                if (_isRunning) return;
                _isRunning = true;
                _lastUpdate = DateTime.UtcNow;
                _accumulated = TimeSpan.Zero;
                ConsoleGuiTimer.Tick += OnTick;
                ConsoleGuiTimer.Start();
                Render();
            }
        }
        
        public void Stop()
        {
             lock(_lock)
             {
                 if (!_isRunning) return;
                 _isRunning = false;
                 ConsoleGuiTimer.Tick -= OnTick;
                 Render();
             }
        }

        public void Dispose()
        {
            Stop();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            lock(_lock)
            {
                if (!_isRunning) return;
                
                var now = DateTime.UtcNow;
                var delta = now - _lastUpdate;
                _lastUpdate = now;
                
                _accumulated += delta;
                
                if (_accumulated >= _spinner.Interval)
                {
                    _accumulated = TimeSpan.Zero;
                    _frameIndex = (_frameIndex + 1) % _spinner.Frames.Count;
                    Render();
                }
            }
        }

        public override Cell this[Position position]
        {
            get
            {
                lock(_lock)
                {
                    if (_bufferConsole.Buffer == null) return new Cell(Character.Empty);
                    if (position.X < 0 || position.X >= Size.Width || position.Y < 0 || position.Y >= Size.Height) return new Cell(Character.Empty);
                    return _bufferConsole.Buffer[position.X, position.Y];
                }
            }
        }

        protected override void Initialize()
        {
            lock(_lock)
            {
                var targetSize = MaxSize;
                if (targetSize.Width > 1000) targetSize = new ConsoleGuiSize(1000, targetSize.Height);
                if (targetSize.Height > 1000) targetSize = new ConsoleGuiSize(targetSize.Width, 1000);

                Resize(targetSize);
                _bufferConsole.Resize(Size);
                
                Render();
            }
        }

        private void Render()
        {
            if (Size.Width <= 0 || Size.Height <= 0) return;
            
            _ansiConsole.Clear(true);
            
            var frame = _spinner.Frames[_frameIndex % _spinner.Frames.Count];
            var frameMarkup = $"[{_style.ToMarkup()}]{Markup.Escape(frame)}[/]";
            _ansiConsole.Markup(frameMarkup);
            
            if (!string.IsNullOrEmpty(_text))
            {
                _ansiConsole.Write(" ");
                _ansiConsole.Markup(_text);
            }
            
            Redraw();
        }
    }
}