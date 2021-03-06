CIPP - Compact Image Processing Platform
===================================
Version 1.2 Released by [Alexandru Dorobanțiu](http://alex.dorobantiu.ro) on 01.12.2020

### Features
- parallel and distributed image processing
- pluggable architecture, explicitly designed for developing algorithms
- simple Design Space Exploration through parameter combinatorics
- contains example plugins for various consecrated image processing algorithms (more will be added)

### Technology:
 - Written in plain **C#** using **Visual Studio**
 - **.NET** framework is kept to version **4.6.2** for compatibility with older operating systems and Visual Studio versions.
 - The front-end is implemented using Windows Forms
 
#### Details
The purpose of the platform is to separate the boilerplate code needed to view, load, save images from the implementation of image processing algorithms. The execution of the algorithms is also managed independently of the implementation. This separation is done using interfaces and a pluggable architecture.

CIPP supports three types of **image processing tasks**
1. Filtering: Takes an image as input and outputs another image (most processing tasks fall into this category)
2. Masking: Takes an image as input and outputs an alpha channel (which applied to the original image results in a masked image)
3. Motion detection: Block matching in sequential frames, takes a sequence of images as input and outputs a series of motion vectors (which describe where the best block match was found in the succeeding image)

#### Implementation Details
Disclaimer: The initial version of the program was developed during my Bachelor's Thesis in Computer Science and the code was mostly organised in my coding standards back then (2009-2010), which were not intended to be compatible with industry style coding. The coding and naming conventions are a mix between C# and Java style (in a good sense).

 - The main project is **CIPP** which is a **Windows Application** using Windows Forms.
 - The secondary project is **CIPPServer** which is a **Console Application** used for distributed processing over TCP/IP.
 - The **ProcessingImageSDK**, **ParametersSDK** and **Protocols** projects separates reusable code between the Client and Server application. The first two should also be referenced by the plugins. These projects are **Class Libraries**. 
 - The **FilterSDK**, **MaskSDK** and **MotionRecognitionSDK** projects are used for developing the three types of plugins and contain the interface the plugin must implement. Reference one of these in your plugin project. These projects are **Class Libraries**.
 - The rest of the projects contain various plugin implementations and are **Class Libraries**.

##### Notes
 - The plugins are loaded from a fixed folder relative to the application. In order to have a plugin build copied automatically to the specified folder, see the Post-build event command line (project -> properties -> Build Events tab) in the example plugins. The script copies the *.dll* file (which is the output of a class library) to the plugins folders of both the client and the server applications.
 - The framework code is using unsafe C# code for fast loading, creating, saving and transforming Bitmaps, but I strongly discourage the use of unsafe code in any of the plugins.
 - The solution should be easy to migrate to newer Visual Studio environments without any changes.
 - The platform does not come with an installation package (ready for execution).

##### Screenshot
![CIPP Screenshot](cipp_printscreen.png)
 
[![Creative Commons License](https://i.creativecommons.org/l/by-nc-sa/4.0/88x31.png)][CreativeCommonsLicence]
<br />
This work is licensed under a [Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License][CreativeCommonsLicence]

[CreativeCommonsLicence]: http://creativecommons.org/licenses/by-nc-sa/4.0/
 