# Windows-nRF-Toolbox
nRF Toolbox source code for release 1.0.0

The nRF Toolbox is a container app that stores your Nordic Semiconductor apps for Bluetooth Smart in one location.  

It contains applications demonstrating Bluetooth Smart profiles: 
* **Heart Rate Monitor**, 
* **Proximity Monitor**. 

### Device Firmware Update
The Device Firmware Update (DFU) profile allows you to update the application, bootloader and/or the Soft Device image over-the-air (OTA). It is compatible with Nordic Semiconductor nRF51822, nRF51422 devices that have the S110 SoftDevice and bootloader enabled. To create initial package by using [nrf utility](http://developer.nordicsemi.com/nRF51_SDK/nRF51_SDK_v8.x.x/doc/8.0.0/s110/html/a00092.html)

The DFU has the following features:
* **Scans for devices that are in DFU mode**, 
* **Connects to devices in DFU mode and uploads the selected firmware (soft device, bootloader and/or application).**. 
* **Allows BIN file upload through your phone or tablet.**. 
* **Includes pre-installed examples that consist of a blinky firmware.** 

##Known issue
* **Windows Phone OS 8.1 is not able to run DFU at this moment, kindly waiting for update from Microsoft.**


### Note
- Requires Windows Phone 8.1 or newer.
- Tested on Lumia 930, Lumia 635
- Compatible with nRF51822 devices that have S110 v5.2.1+ and the bootloader from nRF51 SDK v4.4.1+
- nRF51822 Development kits can be ordered from http://www.nordicsemi.com/eng/Buy-Online.
- The nRF51 SDK and S110 SoftDevice are available online at http://www.nordicsemi.com
