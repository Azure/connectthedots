Get the Bluetooth stack on Raspbian:

sudo apt-get install bluetooth bluez-utils blueman
sudo apt-get install python-bluez

Enable the Bluetooth device

sudo hciconfig hci0 up

If the device ever gets disconnected try:

sudo hciconfig hci0 down

followed by:

sudo hciconfig hci0 up
