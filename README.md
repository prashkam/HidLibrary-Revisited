HidLibrary-Revisited
====================

This library is originally created by Mike O'Brien, but he decided to no longer maintain it.  While I am learning C# I noticed that this library is unable to scan for newly inserted devices. So, I modified HidDevices class by adding a timer that enables the software using this library to scan for newly inserted devices.  I also modified HidDevice class to return the serial number of the device (if exists) rather than the device path. The motive behind modifying this library was to make it possible to use it with multiple devices that have the same VID but different serial numbers.
