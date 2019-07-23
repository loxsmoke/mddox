# mddox

Global tool to create simple markdown documentation using reflection and XML comments that are extracted from the source code by the compiler.

[![XML documentation comments on MSDN](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/xml-documentation-comments)]

[![Sample documentation](https://github.com/loxsmoke/DocXml/blob/master/api-reference.md)]


## Installation

```bash
dotnet install tool -g loxsmoke.mddox
```

## Uninstallation

```bash
dotnet tool uninstall -g loxsmoke.mddox
```

## Usage

```bash
mddox
Usage: mddox <assembly> \[optional-paramters\]

<assembly>   - The name of the assembly to document.

Optional parameters:
  --output <output_md>   The name of the markdown output file.
  --format <format>      The markdown file format. Valid values: github, bitbucket
  --recursive            Step into referenced assemblies recursively.
  --recursive <assembly> Step recursivelly only into specified assembly or assemblies.
  --ignore-methods       Do not generate documentation for methods and constructors.
  --ignore-attribute <name>
                         Do not generate documentation for properties with specified custom attribute.
                         For example JsonIgnoreAttribute
  --type <name>          Document specified only this and referenced types.

```

Documenting all types of one assembly

```bash
mddox MyAssembly.dll
```

Documenting only fields and properties of all types in assembly

```bash
mddox MyAssembly.dll --ignore-methods
```

Documenting types that do not have specified custom attributes

```bash
mddox MyAssembly.dll --ignore-attribute JsonIgnoreAttribute --ignore-attribute XmlIgnore
```

Document one type and all referenced types from different assemblies 

```bash
mddox MyAssembly.dll --type ClassToDocument --recursive ReferencedAssembly1.dll --recursive ReferencedAssembly2.dll
```
