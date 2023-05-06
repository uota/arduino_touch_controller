# Arduino touch controller for Windows
This project provides source code and methods to build a touch controller for Windows using Arduino Nano. This controller allows users to operate Windows applications using touch panels. The architecture of the controller is shown in the figure below.

![archtecture](https://user-images.githubusercontent.com/4375451/236595483-032ebebf-071c-4ff7-b14a-1e42987d9b72.png) 

8 metal touch panels are connected to the Arduino. When a user touches a panel, the firmware on the Arduino detects the touch state and sends it to the Windows PC via serial communication. The serial-keyevent converter on the Windows PC receives the touch state and generates a virtual keyboard press event. Finally, Windows applications (e.g., game application) receives the press event.

## Hardware assembly
### Parts
* Arduino Nano x 1
* touch panel x 8
  * This controller uses [Capacitive sensing](https://en.wikipedia.org/wiki/Capacitive_sensing). Therefore, the touch panel must be conductive (I used a 1.0 mm thick aluminum plate). You can cut the panel to any size you like.
* 1M ohms resistor x 8
* electric wire (to connect Arduino and touch panels)

### Circuit diagram
![circuit diagram](https://user-images.githubusercontent.com/4375451/236595486-e8f42bc2-1add-4a36-a083-a25d42b087ab.png) 

## Building software
### Firmware
1. Download [Arduino IDE](https://www.arduino.cc/en/software).
2. Connect your PC and Arduino with a USB cable.
3. Open the firmware (`arduino_touch_controller.ino`) with Arduino IDE.
4. Select `Arduino Nano` in `Tools`>`Board` menu.
5. Select Arduino serial port in `Tools`>`Port` menu (you can get the port number using Windows device manager).
6. Press Ctrl+U to compile the firmware and upload it to your Arduino.

### Serial-keyevent converter
1. Compile the serial-to-keyboard event converter (`serial_keyevent_converter.cs`) using the following command. You can use the compiler (`csc`) that comes with the .NET framework. If the compilation is successful, `serial_keyboard_converter.exe` will be generated.

   ```
   C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc serial_keyevent_converter.cs
   ```
   **Note**: You may have to change the `csc` path because the `csc` path varies depending on your Windows environment.

## Running
1. Connect your PC and Arduino with a USB cable.
2. Run `serial_keyboard_converter.exe` from the command prompt.
   ````
   serial_keyboard_converter.exe
   ````
3. Enter the serial port number of the Arduino (you can get the port number using Windows device manager).
   ```
   Select the serial port to which the Arduino is connected.
   #1: COM3
   #2: COM4
   #3: COM7
   Please select a number >> 3
   ````
4. While the key is touched, key press events of {Q,W,E,R,T,U,I,O} will be published on Windows. For example, you can see the characters being typed into a text editor. You can also play games with this controller by changing the key bindings of the game application.
5. Pressing enter in the command prompt will exit the `serial_keyboard_converter.exe`.

## Settings
### Adjusting firmware variables
* If the controller does not work well, you need to adjust the variables in the firmware (`arduino_touch_controller.ino`).

#### THRESHOLD_COUNT_UNTOUCH
* The reconfirmation count to transition to non-touch state.
  * Increasing this value will stabilize the detection of continuous touches.
  * Decreasing this value will reduce the delay in detecting non-touch status. (However, this is not important for most applications).

#### CHARGE_TIME_MICRO_SECOND
* The charging time for finger capacitors.
  * Increasing this value will make it easier to detect touches, but it increases over-detection.
  * Decreasing this value will prevents over-detection, but it makes it harder to detect touches.

### Key bindings
* Key bindings can be changed by editing the `KEY_ASSIGN_LIST` array in `serial_keyevent_converter.cs`.
