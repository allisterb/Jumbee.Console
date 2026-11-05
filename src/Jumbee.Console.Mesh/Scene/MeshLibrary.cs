namespace Jumbee.Console.Mesh;

using System.Threading;

/// <summary>An ordered list of named meshes that a viewer steps through and a model picker lists.</summary>
/// <remarks>
/// <para>
/// Owned by the app, not global: two views can show two libraries, and a library nothing refers to is collected with
/// its meshes. A frame never indexes into one — it holds the <see cref="TriangleMesh"/> it draws — so a library
/// changing cannot disturb a frame already published.
/// </para>
/// <para>
/// <b>Append-only and copy-on-write.</b> Each <see cref="Add"/> publishes a new array, so a reader on any thread sees
/// the old list or the new one, both complete, and an index stays valid for the library's lifetime. Add from one
/// thread at a time — the UI thread, in practice: a background loader parses, then hands its meshes over.
/// </para>
/// </remarks>
public sealed class MeshLibrary
{
    #region Indexers
    /// <summary>The mesh at <paramref name="index"/>.</summary>
    public TriangleMesh this[int index] => Volatile.Read(ref entries)[index].Mesh;
    #endregion

    #region Properties
    /// <summary>How many meshes the library holds.</summary>
    public int Count => Volatile.Read(ref entries).Length;
    #endregion

    #region Methods
    /// <summary>Appends <paramref name="mesh"/> under <paramref name="name"/> and returns its index.</summary>
    public int Add(TriangleMesh mesh, string name)
    {
        var current = Volatile.Read(ref entries);
        Volatile.Write(ref entries, [.. current, (mesh, name)]);
        return current.Length;
    }

    /// <summary>The display name the mesh at <paramref name="index"/> was added under.</summary>
    public string NameOf(int index) => Volatile.Read(ref entries)[index].Name;

    /// <summary>The index of the first mesh added under <paramref name="name"/>, or -1.</summary>
    public int IndexOf(string name) => Array.FindIndex(Volatile.Read(ref entries), e => e.Name == name);

    /// <summary>Every name, in index order.</summary>
    public string[] Names() => [.. Volatile.Read(ref entries).Select(e => e.Name)];
    #endregion

    #region Fields
    private (TriangleMesh Mesh, string Name)[] entries = [];   // replaced whole, never edited — see the remarks
    #endregion
}
