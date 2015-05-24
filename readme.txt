Build order

- make the openvg first, this is the C library that is used to interop with OpenVG
  The make will automatically copy the output to the ../OpenVGRenderer/OpenVGRenderer/lib/ folder

- then make the OpenVGRenderer(../OpenVGRenderer/OpenVGRenderer), this is the C++ code that manages composition of the the screen shapes
  and images. Make will copy the output to the ../VGMenu/ folder, which contains the C# code.
  Note that a vcproj exists for visual studio, but because it relies on libraries used on the rpi you won't be able to compile it, I used visual studio
  as the editor and compiled it with the makefile on rpi.

- You will probably need to recompile the cec-client if you're not using the armel release, because it won't be compatible with raspbian out of the box.

- The C# project consists of the VGMenu class library, which contains all the management for episodes and movies and screen navigation. There is a Program.cs in there for testing purposes (so I could test the layout in a winforms form with the TestSHapeController), but the main application entry is the Player console project. I've integrated it with my other rpi stuff so I have a remote access to control the player. I didn't include that but if you comment out the OMXPlayerManager it should work fine.

Once that is built you can run it with mono on the rpi and it should hopefully display the menu. Depending on how many images are on the screen you might have to increase the gpu memory in the rpi settings, otherwise you'll get an out of memory error from OpenVG.

Don't forget to adjust the settings in the app.config of the Player project to be valid for your setup.