# wskia

This is an example using Skia and SkiaSharp to create some graphics using C# and NativeAOT-LLVM



* WebGL - Updated for WebGL.  The `WEBGL` macro is now set and the compile line is updated to use a later version of Skia and drop the JS blit code: 
`dotnet publish /p:SelfContained=true -r browser-wasm -c Debug /p:TargetArchitecture=wasm /p:PlatformTarget=AnyCPU /p:MSBuildEnableWorkloadResolver=false  --self-contained /p:EmccExtraArgs=".packages\skiasharp.nativeassets.webassembly\2.88.0-preview.179\build\netstandard1.0\libSkiaSharp.a\2.0.23\libSkiaSharp.a"`

