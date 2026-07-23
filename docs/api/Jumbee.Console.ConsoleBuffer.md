# <a id="Jumbee_Console_ConsoleBuffer"></a> Class ConsoleBuffer

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A ConsoleGUI.IConsole implementation that writes to a buffer.

```csharp
public class ConsoleBuffer
```

#### Inheritance

object ← 
[ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

## Properties

### <a id="Jumbee_Console_ConsoleBuffer_KeyAvailable"></a> KeyAvailable

Always <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>; input is handled by the input listeners, not this buffer.

```csharp
public bool KeyAvailable { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_ConsoleBuffer_Size"></a> Size

The logical size of the buffer; setting it resizes the backing cell arrays.

```csharp
public Size Size { get; set; }
```

#### Property Value

 Size

### <a id="Jumbee_Console_ConsoleBuffer_Item_ConsoleGUI_Space_Position_"></a> this\[Position\]

Gets the cell at <code class="paramref">position</code>.

```csharp
public Cell this[Position position] { get; }
```

#### Property Value

 Cell

### <a id="Jumbee_Console_ConsoleBuffer_Item_System_Int32_System_Int32_"></a> this\[int, int\]

Gets the cell at column <code class="paramref">x</code>, row <code class="paramref">y</code>.

```csharp
public Cell this[int x, int y] { get; }
```

#### Property Value

 Cell

## Methods

### <a id="Jumbee_Console_ConsoleBuffer_AddX_ConsoleGUI_Space_Position_System_Int32_"></a> AddX\(Position, int\)

Advances <code class="paramref">pos1</code> by <code class="paramref">x</code> cells in row-major order, wrapping across rows.

```csharp
public Position AddX(Position pos1, int x)
```

#### Parameters

`pos1` Position

`x` int

#### Returns

 Position

### <a id="Jumbee_Console_ConsoleBuffer_CharacterAt_System_Int32_System_Int32_"></a> CharacterAt\(int, int\)

The <xref href="ConsoleGUI.Data.Character" data-throw-if-not-resolved="false"></xref> at (<code class="paramref">x</code>, <code class="paramref">y</code>) — a cell read without the
    surrounding <xref href="ConsoleGUI.Data.Cell" data-throw-if-not-resolved="false"></xref> (no mouse-listener copy), for a hot draw loop reading its own output back.

```csharp
public Character CharacterAt(int x, int y)
```

#### Parameters

`x` int

`y` int

#### Returns

 Character

### <a id="Jumbee_Console_ConsoleBuffer_GetPosition_System_Int32_"></a> GetPosition\(int\)

Converts a linear cell <code class="paramref">distance</code> (row-major) into an (x, y) <xref href="ConsoleGUI.Space.Position" data-throw-if-not-resolved="false"></xref>.

```csharp
public Position GetPosition(int distance)
```

#### Parameters

`distance` int

#### Returns

 Position

### <a id="Jumbee_Console_ConsoleBuffer_Initialize"></a> Initialize\(\)

Fill buffer with empty/transparent cells.

```csharp
public void Initialize()
```

### <a id="Jumbee_Console_ConsoleBuffer_OnRefresh"></a> OnRefresh\(\)

No-op; the buffer is composited by the renderer rather than refreshing itself.

```csharp
public void OnRefresh()
```

### <a id="Jumbee_Console_ConsoleBuffer_ReadKey"></a> ReadKey\(\)

Will be handled by IInputListeners.

```csharp
public ConsoleKeyInfo ReadKey()
```

#### Returns

 ConsoleKeyInfo

### <a id="Jumbee_Console_ConsoleBuffer_Resize_ConsoleGUI_Space_Size_"></a> Resize\(Size\)

Resizing the control dimensions resizes the console buffer.

```csharp
protected void Resize(Size size)
```

#### Parameters

`size` Size

### <a id="Jumbee_Console_ConsoleBuffer_Write_ConsoleGUI_Space_Position_ConsoleGUI_Data_Character__"></a> Write\(Position, in Character\)

Sets the console buffer cell character.

```csharp
public void Write(Position position, in Character character)
```

#### Parameters

`position` Position

`character` Character

### <a id="Jumbee_Console_ConsoleBuffer_Write_System_Int32_System_Int32_ConsoleGUI_Data_Character__"></a> Write\(int, int, in Character\)

Writes a character at column <code class="paramref">x</code>, row <code class="paramref">y</code> — the
    <xref href="ConsoleGUI.Api.IConsoleBuffer" data-throw-if-not-resolved="false"></xref> render-target entry point (used by controls that draw straight into the buffer).

```csharp
public void Write(int x, int y, in Character character)
```

#### Parameters

`x` int

`y` int

`character` Character

### <a id="Jumbee_Console_ConsoleBuffer_Write_System_Int32__System_Int32__ConsoleGUI_Data_Cell__"></a> Write\(in int, in int, in Cell\)

Sets the console buffer cell character.

```csharp
public void Write(in int X, in int Y, in Cell cell)
```

#### Parameters

`X` int

`Y` int

`cell` Cell

### <a id="Jumbee_Console_ConsoleBuffer_Write_ConsoleGUI_Space_Position_ConsoleGUI_Data_Cell__"></a> Write\(Position, in Cell\)

Sets the console buffer cell character.

```csharp
public void Write(Position position, in Cell cell)
```

#### Parameters

`position` Position

`cell` Cell

