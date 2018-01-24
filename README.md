CIPP - Compact Image Processing Platform
===================================
Version 1.1 Released by [Alexandru Dorobanțiu](http://www.alex.dorobantiu.ro) on 24.01.2018

### Features
- parallel and distributed image processing
- pluggable architecture, explicitly designed for developing algorithms
- simple Design Space Eploration through parameter combinatorics.
- cotains example plugins for various consacrated image processing algorithms (more will be added)

### Technology:
 - Written in plain **C#** using **Visual Studio 2010**
 - **.NET** framework is kept to version **2.0** for compatibility with older operating systems and Visual Studio versions. It also encourages writing simple and portable code to other programming languages without abusing syntactic sugar provided by newer C# extensions
 - The frontend is implemented using Windows Forms
 
#### Details
The purpose of the platform is to separate the boilerplate code needed to view, load, save images from the implementation of image processing algorithms. The execution of the algorithms is also manged independently of the implementation. This separation is done using interfaces and a pluggable architecture.

CIPP supports three types of **image processing tasks**
1. Filtering: Takes an image as input and outputs another image (most processing tasks fall into this category)
2. Masking: Takes an image as input and outputs an alpha channel (which applied to the original image results in a masked image)
3. Motion detection: Block matching in sequential frames, takes a sequence of images as input and outputs a series of motion vectors (which describe where the best block match was found in the successing image)

#### Implementation Details
Disclaimer: The initial version of the program was developed during my Bachelor's Thesis in Computer Science and the code was mostly organised in my coding standars back then which were not intended to be compatible with industry style coding. The naming conventions are a mix between C# and Java style (in a good sense).

 - The main project is **CIPP** which is a **Windows Application**.
 - The secondary project is **CIPPServer** which is a **Console Application** used for distributed processing.
 - The **ProcessingImageSDK**, **ParametersSDK** and **Protocols** projects separates reusable code between the Client and Server application. The first two should also be referenced by the plugins. These projects are **Class Libraries**. 
 - The **FilterSDK**, **MaskSDK** and **MotionRecognitionSDK** projects are used for developing the three types of plugins and contain the interface the plugin must implement. Reference one of these in your plugin project. These projects are **Class Libraries**.
 - The rest of the projects contain various plugin implementations and are **Class Libraries**.

##### Notes
 - The plugins are loaded from a fixed folder relative to the application. In order to have a plugin build copied automatically to the specified folder, see the Post-build event command line (project -> properties -> Build Events tab) in the example plugins. The script copies the *.dll* file (which is the output of a class library) to the plugins folders of both the client and the server applications.
 - The framework code is using unsafe C# code for fast loading, creating, saving and trasforming Bitmaps, but I strongly discourage the use of unsafe code in plugins.
 - The solution should be easy to migrate to newer Visual Studio environments withoud any changes.
 - The platform does not come with an installation package (ready for execution).

##### Screenshot
![CIPP Screenshot](images/cipp_printscreen.png)
 
[![Creative Commons License](https://i.creativecommons.org/l/by-nc-sa/4.0/88x31.png)][CreativeCommonsLicence]
<br />
This work is licensed under a [Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License][CreativeCommonsLicence]

[CreativeCommonsLicence]: http://creativecommons.org/licenses/by-nc-sa/4.0/
 