# XELF.Imaging
* Limited-use supports of image reader/writer for Unity: APNG/DDS 

* This project is intented to be used for the specific purpose.
  * Note that the supported features are limited.
* Not contained APNG/DDS importer/exporter as an asset.
  * Low-level scripting features only.
  * APNG/DDS importer/exporter will be supported by a higher-level project.

## Specifications
* APNG reader / writer (limited)
  * APNG is suppported Zlib/DEFLATE compression via .NET `System.IO.Compression.DeflateStream`.
  * Supports of animation are limited.
* DDS writer (limited)
* Written in C#6
  * Required `Script Runtime Version`: `.NET 4.x Equivalent`

## Limitations (in current versions)
* 8 bits/pixel only.
* PNG `IDAT` chunk and APNG `fdAT` chunk can read/write once only.
  * Can only read/write low resolution images.
  * Cannot read an image contained verbose chunks.
* DDS reader does not exist.
* PNG filter method 0 is only supported filter type `None`.

## Install manually

* Copy this project files into your local project folder.

## Install via Package Manager

* Temporal `package.json` is contained for Package Manager.
  * For more informations ☛ [XELF.Hexelate.Shader](https://github.com/xelfia/XELF.Hexelate.Shader)

