# Based on the Adafruit CircuitPython example

import time
import board
import digitalio
from adafruit_hid.keyboard import Keyboard
from adafruit_hid.keyboard_layout_us import KeyboardLayoutUS
from adafruit_hid.keycode import Keycode

keyboard = Keyboard()
keyboard_layout = KeyboardLayoutUS(keyboard) 
 
# Sleep for a bit to avoid a race condition on some systems
time.sleep(1)

pin = digitalio.DigitalInOut(board.D2)
pin.direction = digitalio.Direction.INPUT
pin.pull = digitalio.Pull.UP

# Activate the red led on the board when the button
# is pressed for some quick feedback
internalLed = digitalio.DigitalInOut(board.D13)
internalLed.direction = digitalio.Direction.OUTPUT
internalLed.value = False

externalLed = digitalio.DigitalInOut(board.D1)
externalLed.direction = digitalio.Direction.OUTPUT
externalLed.value = False

print("Waiting for button push...")

while True:
    # Wait for the pin to be grounded 
    if not pin.value: 

        # Sleep to debounce
        time.sleep(0.05)

        # Turn on the red LEDs
        internalLed.value = True
        externalLed.value = True

        # Wait for the pin to no longer be
        while not pin.value:
            pass

        # Prepare the keystrokes we want to send and send them
        keyboard.press(Keycode.SHIFT, Keycode.CONTROL, Keycode.F12)
        keyboard.release_all()

        print("Sending keystroke...")

        # Turn off the red LEDs
        internalLed.value = False
        externalLed.value = False

    time.sleep(0.01)
