-- premake5.lua
workspace "M2PhysX"
   configurations { "Debug", "Release" }
   platforms { "Win32" }

project "M2PhysX"
   language "C++"
   targetdir "bin/%{cfg.buildcfg}"
   staticruntime "on"
   defines { "WIN32" }
   includedirs { 
   "$(ProjectDir)" ,
   "$(ProjectDir)vendors/Cooking/Include",
   "$(ProjectDir)vendors/Foundation/Include",
   "$(ProjectDir)vendors/Physics/Include",
   "$(ProjectDir)vendors/PhysXLoader/Include",
   }

   libdirs {
      "$(ProjectDir)vendors/libs"
   }

   files { "**.h", "**.c" , "**.cpp"}

   filter "configurations:Debug"
      defines { "DEBUG" }
      kind "ConsoleApp"
      architecture "x86"
      symbols "On"

      links {
         "/Win32/NxCharacter.lib",
         "/Win32/NxCooking.lib",
         "/Win32/PhysXLoaderDEBUG.lib",
      }

   filter "configurations:Release"
      defines { "NDEBUG" }
      kind "SharedLib"
      architecture "x86"
      optimize "On"

      links {
         "/Win32/NxCharacter.lib",
         "/Win32/NxCooking.lib",
         "/Win32/PhysXLoader.lib",
      }