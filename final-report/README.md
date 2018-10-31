# genbank-type-provider

> Final-year thesis for engineering

## Premise

TODO

## Instructions

### Report

You will need:
  - pandoc
  - some kind of latex implementation
    - pdftex
    - biblatex
    - biber
    - ...probably others

```sh
cd report
make
open report.pdf
```

To continuously compile in the background, install `watchman` using your
favourite package manager, then run

```sh
watchman-make -p 'src/*.md' '*.tex' -t pandoc copy -p '*.lib' -t all
```

This will execute `make` when modifying any markdown file in `src/` or modifying
the `titlepage.tex` or `template.tex` files, and only remake the whole thing
when necessary.  You can also add then environment variable `DRAFTMODE=1` to
speed up builds further.
