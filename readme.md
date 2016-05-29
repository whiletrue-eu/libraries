# whiletrue libraries
This library was created by me in my private time as a hobby and used withtin the company I worked for. It provides .Net class libraries to be used as basis for applications (.Net Windows applications, but nowadays I also converted large parts of the code to PCL)

### what is it about?
I mainly focused on these things:
* MVVM (Model-View-ViewModel), especially in conqunction with WPF
* Smart Card access for windows (PC/SC)
* WPF extended controls (e.g. specializied windows and panels)

### MVVM
"Aren't there already a ton of MVVM support libraries?"
Yes. and no. What I was searching for in conjunction with MVVM was a possibility to generate true ViewModel classes (i.e. wrapping everything of the model, so that it will never be exposed to the UI) but with the least effort possible.
I was looking at many libraries, many of them operating with attributes for properties written into the view model that were used to auto-generate the code behind those properties by interpreting string-based expressions within those attributes.
This was not enough for me; first of all, those are limited to expressions that the interpreter can understand. More complex expressions cannot be used and have to be implemented automatically. Second, strings are not refactoring friendly.
What my library, especially the 'ObservableObject' class provides is, that you can use standard .Net lambda expressions to address properties from the model, and the library will automatically care about the event handling within the view model.
The event handling is done using weak event handlers. This allows to collect the ViewModel instances when they get disconnected from the UI even when the model insatnces are still alive; with standard handlers, you would get memory leaks because the view model instances are still referenced from the model through their event handler delegates.

Additionally, an IoC (Inverse Of Controls) container is provided to allow modular applications.
I provided an own implementation (adding to the 'thousands' already there) because I had two requirements not fulfilled by any of the ones I reviewed:
* Simple to use: Most contains I have seen are overengineered by providing a way to conect everything to everything else. While this captures all thunkable use cases, it make debugging a mess. When something doesn't work, you're totally lost. So the design of the whiletrue IoC container was done in a simply way (only considering classes and interfaces marked with a special attribute), with appropriate error messages (e.g. for cyclic dependencies).
* Allow differnt lifetimes for different parts of the application: The contains I have reviewed all created the complete component graph during initialisation. But this means, that if you have a dialog only shown seldomly, its backing components (view, viewmodel, optionally some additional services) are created and held in memory even if the dialog is not shown. So, I splitted up the component repository (keeping entries for all components in the application) and the component container(s) (container for the instances), so that the lifetime of the components can be bound to the lifetime of the containers. In this way, a dialog window can be held in a specific container instance which get's disposed when the dialog is closed, disposing all instances that were created within that container. 

There are some more classes to simplify WPF development with MVVM models; just look around to find them ;-)
* Drag & drop support: I found that WPF si a great approach, but some parts still are realised with the same concepts like Windows Forms, breaking the MVVM compatibility. I added some classes that allow to use Drag & Drop in MVVM, seperating the UI and view model completely. It allows to implement the complete functionality with (view)models without caring about UI (e.g, in the implementation within your view model, you only care about view model classes). This also works across applications (optionally implementing serialisation of (view)model objects) and is extensible, to easily support custom controls. Some sugar, like automatical selection of tabs when hovering or auto-scroll during drag operations is already built-in, including user feedback beyond the 'drag cursor'.
* Property persistence in the UI: To persist some options of the UI (e.g. window size and position, grid column width, expanded/collapsed states) you don't have to implement anything. There are some markupextensions which care about saving and restoring the values. Just look into the examples or search for 'PersistentProperty' to see how its done
* Same applies for UI feature management (hide/disable UI based on some business rules, e.g. user privileges), which is realized using simple markus extensions which can be directly applied on the XAML level.
* much more... browse the library code to find it or request some more documentation by filing an issue here

As a sample, there is the MVVM driven 'model inspector' which you can add to your WPF project and register models and/or view models.
The model inspector will show you a sencondary window (nidependent of your application window), which will present the details of the registered models to deep-dive into it. It also will show change notifications and validation results. It uses drag & drop to allow simpler access to sub-models by dragging tree nodes into a new view within the window.


### Smart card
This library implements PC/SC access for windows.
Additionally, it contains a model and control for an ATR editor that allows interpretation and editing on three levels (hex, ATR byte groups, interpreted)

### WPF extended controls
* DialogPanel, an easy to use panel to get a table of controls with a caption on each control automatically layouted
* TablePanel, which supports alignment of columns in the table for nested layouts, so that it is easy to create aligned 'tree-list' views
* NotificationBanner which implements windows style guide like info, warning and error messages
* ValidationResult control which provides popup-style nootifications for errors occuring within a specific input control
* ContentUnavailableControl which is an easy option to display default messages if there is no content available to display
* NotifyIcon, a WPF compatible verision of the windows.forms notifyicon class, that uses a WPF window content as the icon (including animations etc.)
* NotificationWindow, for toast notification appearing near to the system tray
* Splash Screen, which enables animated WPF splash screens while your application is loading and initialilizing
* much more... browse the library code to find it or request some more documentation by filing an issue here

## license
The source code provided in this repository is licensed under MIT license (http://opensource.org/licenses/MIT).
Pull requests for new features or bug fixes are explicitely welcome

The MIT License (MIT)

Copyright (c) 2009-2015 Michael Zimdars, www.WhileTrue.eu

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
