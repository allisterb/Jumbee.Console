# Jumbee.Console Snapshot

## About
This is an add-on library for Jumbee.Console that provides headless snapshot testing: render any control or layout to text or PNG without a real terminal.

## Getting Started
See [GETTING-STARTED.md](https://github.com/allisterb/Jumbee.Console/blob/master/GETTING-STARTED.md) on the project site, and the project [documentation](https://github.com/allisterb/Jumbee.Console/tree/master/docs). 

## Known issue: SixLabors.ImageSharp security advisories
This package depends on SixLabors.ImageSharp 3.1.8 (through SixLabors.ImageSharp.Drawing 2.1.6), which has known security advisories, so restoring a project that uses Jumbee.Console.Snapshot reports NU1903 and NU1902 warnings:

- four high-severity, published October 2026: GHSA-j3p4-wp97-rph4, GHSA-j9gm-c75j-xc9q, GHSA-jjfr-hcj7-qf5w, GHSA-v76p-62qx-wwq2;
- three moderate-severity: GHSA-gwg2-r3hj-4w44, GHSA-rxmq-m78w-7wmc, GHSA-wmxv-xphr-5c9g.

The vulnerable code is in ImageSharp's image decoders (TIFF, BigTIFF, GIF, and embedded ICC colour profiles), its TIFF encoders, and its histogram equalization. Jumbee.Console.Snapshot uses none of it: it never reads an image, only creates one, draws text on it and writes it as a PNG. It is also a testing library, normally referenced only by test projects.

The fixed ImageSharp releases (3.2.0 and later) require a Six Labors licence key to build, so updating them is planned for a later release. If your own code uses ImageSharp's TIFF support or histogram equalization, reference a fixed ImageSharp version directly.
