# wskia

This is an example using Skia and SkiaSharp to create some graphics using C# and NativeAOT-LLVM

Compile with `dotnet publish /p:SelfContained=true -r browser-wasm -c Debug /p:TargetArchitecture=wasm /p:PlatformTarget=AnyCPU /p:MSBuildEnableWorkloadResolver=false  --self-contained /p:EmccExtraArgs=".packages\skiasharp.nativeassets.webassembly\2.80.3\build\netstandard1.0\libSkiaSharp.a\2.0.12\libSkiaSharp.a --js-library Context.js"`

I couldn't get WebGL working so this is software only.  That's either because I don't know what I'm doing (most likely) or there is a problem with SkiaSharp or the Skia WebASsembly build.  I created https://github.com/mono/SkiaSharp/issues/1931 which is where I got to.


