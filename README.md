# SVG to VectorDrawable Converter
Batch converter of SVG images to Android VectorDrawable XML resource files.

Put the output XML files into the ‘res/drawable’ directory of your app and reference to them in XML / Java code as to ordinary drawables.

### **Online version** of the converter (not batch) is [here](http://a-student.github.io/SvgToVectorDrawableConverter.Web/).

## Requirements
Inkscape (free and open-source vector graphics software) must be installed. Download it from https://inkscape.org/en/download
After installation, make sure the application runs correctly.

The converter can be downloaded
[here ![releases](https://img.shields.io/github/release/a-student/SvgToVectorDrawableConverter.svg)](https://github.com/a-student/SvgToVectorDrawableConverter/releases/latest)

## Running on Windows
.NET Framework 4.5 or higher is required. Usually it is already installed on your operating system (enabled by default in Windows 8/10).
If you have older OS, please search the Microsoft web site for .NET Framework, download and install it.

Open command prompt and run
```
svg2vd.exe --help
```
This command prints usage and option explanations.

Typical usage:
```
svg2vd.exe -i *
```
(Converts all SVG files in current directory.)

## Running on OS X
OS X 10.7 or better, Mono and Inkscape are required. If you do not have Mono installed, please download and install it from http://www.mono-project.com/download
For Inkscape installation instructions visit https://inkscape.org/en/download/mac-os/

Or if you use [Homebrew](http://brew.sh/) just run in terminal:
```
brew install mono homebrew/gui/inkscape
```

This command prints usage and option explanations:
```
mono svg2vd.exe --help
```

Typical usage:
```
mono svg2vd.exe -i \*
```
(Converts all SVG files in current directory.)

## Running on Linux
Mono and Inkscape are required:

```
sudo apt-get install mono-devel inkscape
```
The converter is known to work well with Mono version 4.6.1 and Inkscape version 0.91. To check the vesions installed use the `mono --version` or `inkscape --version` commands respectively. The latest Mono [installation guide](http://www.mono-project.com/download/#download-lin), get the latest Inkscape [here](https://inkscape.org/en/download/linux/)

Open terminal and run
```
./svg2vd.exe --help
```

or

```
mono svg2vd.exe --help
```

This command prints usage and option explanations.

Typical usage:
```
mono svg2vd.exe -i \*
```
(Converts all SVG files in current directory.)


## Previewing vector drawables
Android Studio is able to open vector drawables. Note that it does not always handle fill-rule correctly.

## Not supported SVG features
These SVG elements are not supported by VectorDrawable: **patterns, masks, gradients, images,** etc.

VectorDrawable **fill-rule** is always **non-zero** and cannot be changed prior to Android 7.0 (Nougat).
If you end up with areas filled that should not be filled, that is because the SVG image was created using even-odd rule instead.
There are three ways to deal with this problem: try specifying the `--fix-fill-type` option, manually edit SVGs in vector graphics software or convert for Android 7.0+.

### Specifying the `--fix-fill-type` option
This option is experimental, but it is worth trying first. The option uses complex mathematics to rebuild paths in such a way that rendering with any fill-rule leads to the same result.

### Manual editing
If you are using Inkscape, open *Object* → *Fill and Stroke…* and in the *Fill* tab choose *Fill is solid unless a subpath is counterdirectional.*
If you see improperly filled area after this operation, using F2 tool select subpath around that area and apply *Path* → *Reverse* command from the main menu.

If you are using Sketch, select the path, right to the *Fills* property title there is settings icon, click it and choose *Non-Zero* option.
To reverse the path direction apply *Layer* → *Paths* → *Reverse Order* command from the main menu.

### Converting for Android 7.0
This option does not fit all, because it requires setting `minSdkVersion` to 24 at least (Android 7.0 or later) in ‘build.gradle’.
To produce vector drawables for Android 7.0, append to the converter command the `--lib ApiLevel24` argument like
```
svg2vd.exe -i * --lib ApiLevel24
```
This command creates vector drawables containing additional attributes that are understandable by Android 7.0+.

## Issues
If you have any problems with the converter, please create an issue on GitHub (https://github.com/a-student/SvgToVectorDrawableConverter/issues/new),
explain the reproducing steps, and add link to the SVG file (link is optional but highly recommended).

## Alternatives
Less powerful online converter http://inloop.github.io/svg2android
