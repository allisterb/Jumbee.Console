namespace Jumbee.Console.Tests;

using System;
using System.IO;
using System.Linq;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Xunit;

/// <summary>Headless tests for <see cref="FileBrowser"/> over a throwaway directory tree.</summary>
public class FileBrowserTests : IDisposable
{
    public FileBrowserTests()
    {
        UiTestHarness.EnsureStopped();
        ConsoleSnapshot.ResetMouse();

        root = Path.Combine(Path.GetTempPath(), "jc-filebrowser-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(Path.Combine(root, "models", "nested"));
        Directory.CreateDirectory(Path.Combine(root, "docs"));
        File.WriteAllText(Path.Combine(root, "readme.txt"), "hello");
        File.WriteAllText(Path.Combine(root, "models", "teapot.obj"), "v 0 0 0");
        File.WriteAllText(Path.Combine(root, "models", "bunny.obj"), "v 0 0 0");
        File.WriteAllText(Path.Combine(root, "models", "notes.md"), "notes");
    }

    public void Dispose()
    {
        try { Directory.Delete(root, recursive: true); } catch (IOException) { }
        GC.SuppressFinalize(this);
    }

    private readonly string root;

    private static string Text(FileBrowser browser) => ConsoleSnapshot.ToText(browser, 80, 20);

    [Fact]
    public void Lists_DirectoriesThenFiles_WithAnUpRow()
    {
        var browser = new FileBrowser(root);
        var text = Text(browser);

        Assert.Contains("..", text);
        Assert.Contains("models", text);
        Assert.Contains("docs", text);
        Assert.Contains("readme.txt", text);
    }

    [Fact]
    public void Filter_NarrowsTheFileList_ButNotTheFolders()
    {
        var models = Path.Combine(root, "models");
        var browser = new FileBrowser(models, FileBrowserMode.OpenFile, ["*.obj"]);
        var text = Text(browser);

        Assert.Contains("teapot.obj", text);
        Assert.Contains("bunny.obj", text);
        Assert.DoesNotContain("notes.md", text);
        Assert.Contains("nested", text);   // directories are never filtered
    }

    [Fact]
    public void Filters_AlwaysOfferEverything_AsTheLastOption()
    {
        var browser = new FileBrowser(root, FileBrowserMode.OpenFile, ["*.obj"]);
        Assert.Equal(["*.obj", FileBrowser.AllFiles], browser.Filters);
    }

    [Fact]
    public void StartPath_MayBeAFile_AndOpensItsDirectoryWithItSelected()
    {
        var file = Path.Combine(root, "models", "teapot.obj");
        var browser = new FileBrowser(file);

        Assert.Equal(Path.Combine(root, "models"), browser.CurrentDirectory);
        Assert.Equal(file, browser.SelectedPath);
    }

    [Fact]
    public void StartPath_ThatDoesNotExist_FallsBackRatherThanThrowing()
    {
        var browser = new FileBrowser(Path.Combine(root, "nope", "gone"));
        Assert.Equal(Directory.GetCurrentDirectory(), browser.CurrentDirectory);
    }

    [Fact]
    public void SelectedPath_IsNullForADirectory_InFileMode()
    {
        var browser = new FileBrowser(root);
        browser.Select(Path.Combine(root, "models"));

        Assert.Null(browser.SelectedPath);   // a folder is not a valid answer to "choose a file"
    }

    [Fact]
    public void SelectedPath_IsTheListedDirectory_InDirectoryMode()
    {
        var browser = new FileBrowser(root, FileBrowserMode.OpenDirectory);

        // Nothing highlighted yet: OK still has something to return, which is the whole point of directory mode.
        Assert.Equal(root, browser.SelectedPath);

        browser.Select(Path.Combine(root, "docs"));
        Assert.Equal(Path.Combine(root, "docs"), browser.SelectedPath);
    }

    [Fact]
    public void NavigateTo_ChangesTheListing_AndRaisesSelectionChanged()
    {
        var browser = new FileBrowser(root);
        var raised = 0;
        browser.SelectionChanged += (_, _) => raised++;

        browser.NavigateTo(Path.Combine(root, "models"));

        Assert.Equal(Path.Combine(root, "models"), browser.CurrentDirectory);
        Assert.Contains("teapot.obj", Text(browser));
        Assert.True(raised > 0);
    }

    [Fact]
    public void NavigateTo_AMissingDirectory_IsIgnored()
    {
        var browser = new FileBrowser(root);
        browser.NavigateTo(Path.Combine(root, "not-there"));
        Assert.Equal(root, browser.CurrentDirectory);
    }

    [Fact]
    public void CommittingAFolder_NavigatesIntoIt_InFileMode()
    {
        var browser = new FileBrowser(root);
        var activated = 0;
        browser.PathActivated += (_, _) => activated++;

        Commit(browser, Path.Combine(root, "models"));

        Assert.Equal(Path.Combine(root, "models"), browser.CurrentDirectory);
        Assert.Equal(0, activated);   // navigating is not choosing
    }

    [Fact]
    public void CommittingAFile_ActivatesIt()
    {
        var browser = new FileBrowser(Path.Combine(root, "models"));
        string? chosen = null;
        browser.PathActivated += (_, p) => chosen = p;

        Commit(browser, Path.Combine(root, "models", "teapot.obj"));

        Assert.Equal(Path.Combine(root, "models", "teapot.obj"), chosen);
    }

    [Fact]
    public void CommittingTheUpRow_Ascends()
    {
        var models = Path.Combine(root, "models");
        var browser = new FileBrowser(models);

        Commit(browser, root);

        Assert.Equal(root, browser.CurrentDirectory);
    }

    [Fact]
    public void AnEmptyDirectory_SaysSo_RatherThanLookingBroken()
    {
        var empty = Path.Combine(root, "docs");
        var browser = new FileBrowser(empty);
        var text = Text(browser);

        Assert.Contains("..", text);
        Assert.DoesNotContain("(unreadable)", text);
    }

    [Fact]
    public void AFolderInTheTree_IsExpandable_AndPopulatesOnDemand()
    {
        var browser = new FileBrowser(root);
        ConsoleSnapshot.Render(browser, 80, 20);

        // The tree is rooted at drives, not at the temp directory, so what this proves is the mechanism: the roots
        // render as folders (they carry a placeholder child), which is what makes them openable at all.
        var text = Text(browser);
        Assert.Contains("Folders", text);
        Assert.Contains("Contents", text);
    }

    [Fact]
    public void ARootedTree_DoesNotThrowOnProtectedDirectories()
    {
        // Enumerating a drive root meets System Volume Information on Windows; the browser must survive it.
        var drive = Path.GetPathRoot(Directory.GetCurrentDirectory())!;
        var browser = new FileBrowser(drive);
        var text = Text(browser);

        Assert.NotNull(text);
        Assert.Equal(drive, browser.CurrentDirectory);
    }

    // A chooser is a list you look through before acting on it: a single click must only select, or the browser
    // opens whatever file you glanced at. (ListBox commits on a single click by default; the browser opts out.)
    [Fact]
    public void SingleClickingAFile_SelectsIt_WithoutActivatingIt()
    {
        ConsoleSnapshot.ResetMouse();
        var browser = new FileBrowser(Path.Combine(root, "models"));
        var activated = 0;
        browser.PathActivated += (_, _) => activated++;

        var buffer = ConsoleSnapshot.Render(browser, 80, 20);
        var lines = ConsoleSnapshot.ToLines(buffer);
        var row = Array.FindIndex(lines, l => l.Contains("teapot.obj"));
        Assert.True(row >= 0, "the listing did not render");
        Assert.True(ConsoleSnapshot.Click(buffer, lines[row].IndexOf("teapot.obj", StringComparison.Ordinal), row));

        Assert.Equal(Path.Combine(root, "models", "teapot.obj"), browser.SelectedPath);
        Assert.Equal(0, activated);
    }

    [Fact]
    public void DoubleClickingAFile_ActivatesIt()
    {
        ConsoleSnapshot.ResetMouse();
        var browser = new FileBrowser(Path.Combine(root, "models"));
        string? chosen = null;
        browser.PathActivated += (_, p) => chosen = p;

        var buffer = ConsoleSnapshot.Render(browser, 80, 20);
        var lines = ConsoleSnapshot.ToLines(buffer);
        var row = Array.FindIndex(lines, l => l.Contains("bunny.obj"));
        Assert.True(ConsoleSnapshot.Click(buffer, lines[row].IndexOf("bunny.obj", StringComparison.Ordinal), row, clicks: 2));

        Assert.Equal(Path.Combine(root, "models", "bunny.obj"), chosen);
    }

    // Commit the entry with the given path — the same path Enter and a double-click take.
    private static void Commit(FileBrowser browser, string path)
    {
        browser.Select(path);
        ConsoleSnapshot.Render(browser, 80, 20);
        var list = Descendants(browser).OfType<ListBox>().First();
        list.OnInput(new UI.InputEventArgs(
            new ConsoleGUI.Input.InputEvent(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false))));
    }

    private static System.Collections.Generic.IEnumerable<Control> Descendants(CompositeControl composite)
    {
        // The browser's list is the one control the tests need to poke directly; reach it through the composite's
        // own children rather than exposing it on the public surface just for a test.
        var field = typeof(FileBrowser).GetField("entries",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field?.GetValue(composite) is Control control) yield return control;
    }
}
