### 0.0.1 - August 3 2017
* Initial release

### 0.0.2 - August 13 2017
* Added data directories to `OptionalHeader`
* Added `PortableExecutableFile.ResolveExports()`

### 0.0.3 - August 16 2017
* Added `PortableExecutable.ResolveImports()` and related types
* Added `PortableExecutable.OpenSection(string)`
* Moved native type conversions to an internal module
* Implemented section flags
* Added ILMerge to the build workflow