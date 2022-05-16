# FasterQC.net

A C# version of [FastQC](https://github.com/s-andrews/FastQC)

## Usage

```
> dotnet run -- --help

Ovation.FasterQC.Net 1.0.0
Copyright (C) 2022 Ovation.FasterQC.Net

  -v, --verbose     Set output to verbose messages.

  --debug           Show diagnostic output.  Can only use with --verbose.

  -p, --progress    Show progress bar.  Cannnot use with --verbose.

  -i, --input       Required. Input filename.

  -o, --output      Output filename.  Defaults to STDOUT.

  -b, --bam         Assume BAM format.

  -f, --fastq       Assume FASTQ format.

  -z, --zipped      Assume input file is gzipped.

  -m, --modules     Required. Space-separated list of modules to run, or 'all'.

  --help            Display this help screen.

  --version         Display version information.
  ```
