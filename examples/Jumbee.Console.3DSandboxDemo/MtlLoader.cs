namespace Jumbee.Console.SandboxDemo;

using System.Globalization;
using System.Numerics;

/// <summary>A resolved material: the colour a face shows flat, and the baked map it can show instead.</summary>
/// <param name="Name">The name the OBJ's <c>usemtl</c> refers to it by.</param>
/// <param name="Colour">The flat colour: <c>Kd</c>, multiplied by the map's average when there is a map — MTL's
/// own rule, and the reason a <c>Kd 1 1 1</c> material with a map is not drawn white.</param>
/// <param name="Map">The baked diffuse map, or <see langword="null"/> when the material has none.</param>
public sealed record Material(string Name, Color Colour, Texture? Map);

/// <summary>A material as written in an <c>.mtl</c> file, before its map is loaded.</summary>
/// <param name="Name">From <c>newmtl</c>.</param>
/// <param name="Kd">The diffuse colour, each channel 0..1; white when the file gives none.</param>
/// <param name="MapKd">The <c>map_Kd</c> filename as written — relative to the <c>.mtl</c> — or <see langword="null"/>.</param>
public readonly record struct MtlDefinition(string Name, Vector3 Kd, string? MapKd);

/// <summary>
/// A minimal Wavefront MTL reader: <c>newmtl</c>, <c>Kd</c> and <c>map_Kd</c>, which is all the renderers can use.
/// </summary>
/// <remarks>
/// Everything else — specular, ambient, transparency, illumination models, the other maps — is read past. A face
/// here is shaded by one light model whatever the file says; only its base colour, flat or mapped, can vary.
/// </remarks>
public static class MtlLoader
{
    #region Methods
    /// <summary>Reads an <c>.mtl</c> file.</summary>
    public static IReadOnlyList<MtlDefinition> Load(string path) => Parse(File.ReadLines(path));

    /// <summary>Parses MTL text. Split out from <see cref="Load"/> so it can be tested without a file.</summary>
    public static IReadOnlyList<MtlDefinition> Parse(IEnumerable<string> lines)
    {
        var materials = new List<MtlDefinition>();
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line[0] == '#') continue;

            var space = line.IndexOfAny([' ', '\t']);
            var key = space < 0 ? line : line[..space];
            var rest = space < 0 ? "" : line[(space + 1)..].Trim();

            if (key == "newmtl")
            {
                if (rest.Length > 0) materials.Add(new MtlDefinition(rest, Vector3.One, null));
            }
            else if (materials.Count > 0 && key == "Kd")
            {
                // `Kd spectral file` and `Kd xyz x y z` exist in the format and are ignored rather than misread.
                var parts = rest.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3 && TryNumber(parts[0], out var r) && TryNumber(parts[1], out var g) && TryNumber(parts[2], out var b))
                    materials[^1] = materials[^1] with { Kd = Vector3.Clamp(new Vector3(r, g, b), Vector3.Zero, Vector3.One) };
            }
            else if (materials.Count > 0 && key == "map_Kd")
            {
                if (MapFileName(rest) is { } file) materials[^1] = materials[^1] with { MapKd = file };
            }
        }

        return materials;
    }
    #endregion

    #region Private methods
    // `map_Kd [options] filename`. Options come first and take a fixed number of arguments, except -o/-s/-t, which
    // take one to three numbers. Whatever is left is the filename, spaces and all -- exporters do write names with
    // spaces in them. Unknown options are skipped with any numbers following them, rather than read as a filename.
    private static string? MapFileName(string rest)
    {
        var tokens = rest.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        var i = 0;
        while (i < tokens.Length && tokens[i].StartsWith('-') && !TryNumber(tokens[i], out _))
        {
            var option = tokens[i++];
            var arity = option switch
            {
                "-blendu" or "-blendv" or "-cc" or "-clamp" or "-bm" or "-boost" or "-texres" or "-imfchan" or "-type" => 1,
                "-mm" => 2,
                _ => -1,   // -o/-s/-t and anything unknown: consume numbers, up to three
            };
            if (arity > 0) i += arity;
            else for (var n = 0; n < 3 && i < tokens.Length && TryNumber(tokens[i], out _); n++) i++;
        }

        return i < tokens.Length ? string.Join(' ', tokens[i..]) : null;
    }

    private static bool TryNumber(string text, out float value) =>
        float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    #endregion
}
