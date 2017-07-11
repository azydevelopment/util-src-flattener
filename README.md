# util-src-flattener
TLDR: Takes your standard [/include and /src directory structure](https://stackoverflow.com/questions/1398445/directory-structure-for-a-c-library) of a library and flattens it into a single directory while fixing up all the #includes in all of the files so that the flattened structure still builds.

## The context
A library (especially in C/C++) often has a directory structure with /include and /src at the root where the include directory is added to the 'include path'. Ie. The compiler will know to also look in that include folder as a root for files to satisfy #include's.

Additionally, files may be organized into subfolders and included using things like:
```c++
#include <core/dir0/test.h>
```
or
```c++
#include <core/dir1/test.h>
```
Notice the angle brackets since the include is relative to an include path that the compiler knows about ([at least in many common compiler implementations](https://stackoverflow.com/questions/21593/what-is-the-difference-between-include-filename-and-include-filename)). Also notice that one of the benefits of organizing your directories like this is that you can have multiple headers and source files with the same name without ever worrying about your debug.h file colliding with someone elses from a different library you're also using.

## The problem
Anyway, let's say you have the aforementioned folder structure but you really don't want a deep directory for a particular project or you're using tooling that requires everything to be flat. Well all your includes will be wrong if you just copy everything into a flat structure and you might run into file name conflicts.

## The solution
This tool basically takes a target directory argument (ie. your library with /include and /src directories) and an output directory (where you want your flattened files to go), prepends every file with the directory structure delimited by underscores and also fixes up the #include's in all of your files.

## The details
The tool basically does this:
1. Checks you gave it two arguments (which should be the target directory and output directory)
2. Checks that there is both an /include and /src directory in the target directory
3. Finds all the files that the flattening needs to happen to (ie. your .h and .cpp files)
4. Flattens them all by adding it's directory path relative to the target directory, prepends that to the name of the file, and replaces all the includes with that file with the new expected include path

## The example
The Visual Studio solution has a directory called 'test_dir' which is used to demonstrate to you how the tool works. The run arguments for the Debug and Release configurations are set up to use that directory as the test directory. The default is that things in $(SolutionDir)/test_dir/to_be_flattened get flattened into $(SolutionDir)/test_dir/flattened.

A file in:
```
<target directory>/src/core/dir0/test.cpp
```
will become this file:
```
<output directory>/core_dir0_test.cpp
```

The include
```c++
#include <core/dir0/test.h>
```
will become the include:
```c++
#include "core_dir0_test.h"
```
Notice the fact that the angle brackets became quotes. That's because once you flatten things, we don't assume that the flattened directory is directly part of the compiler known include paths.

The tool will also show you what's in your directories before you tell it to go ahead and flatten everything. Here's an example of the output of the tool before it actually goes and flattens everything:
```
PS C:\git\util-src-flattener\util_src_flattener\util_src_flattener\bin\Debug> .\util_src_flattener.exe C:\git\util-src-f
lattener\util_src_flattener\test_dir\to_be_flattened C:\git\util-src-flattener\util_src_flattener\test_dir\flattened
-----------------------------
Flattening source directories...
-----------------------------
Target directory: C:\git\util-src-flattener\util_src_flattener\test_dir\to_be_flattened
Output directory: C:\git\util-src-flattener\util_src_flattener\test_dir\flattened

-----------------------------
Include directory structure:
-----------------------------
\core
  \dir0
    |test.h
  \dir1
    |test.h

-----------------------------
Src directory structure:
-----------------------------
\core
  \dir0
    |test.cpp
  \dir1
    |test.cpp

-----------------------------
Replacement list
-----------------------------
<core/dir0/test.h>                                 --->    "core_dir0_test.h"
<core/dir1/test.h>                                 --->    "core_dir1_test.h"

-----------------------------
Press enter to proceed...
```
