#Miles Drake, UCI, student ID 18154111 6/8/2021

import drivers
import Adafruit_DHT
import threading
import RPi.GPIO as gpio
from time import sleep
import requests
import json

tempHigher = 15                     #initializing pins
tempLower = 18
door = 14
window = 4

redLED = 22
greenLED = 27
blueLED = 17
infraredSensor = 5
DHT = 6

display = drivers.Lcd()

lightMsg = 0
doorwMsg = 0
doorToggle = 0

class GlobalData():       #global data basically acts like a struct, has functions to change these values
    def __init__(self, temp, dtemp, doorw, heat, lights):
        self.temp = temp
        self.dtemp = dtemp
        self.doorw = doorw
        self.heat = heat
        self.lights = lights
        
    def changeTemp(self, new):
        self.temp = new
    def changeDtemp(self, new):
        self.dtemp = new
    def changeDoorw(self, new):
        self.doorw = new
    def changeHeat(self, new):
        self.heat = new
    def changeLights(self, new):
        self.lights = new

globalData = GlobalData(72, 72, 0, 1, 0) #instantiate global struct first so functions can use them

def displayData():       #display the data on main screen using data from global struct
    global display
    global globalData
    
    if globalData.doorw == 0:
        formatString = "{0:0.0f}/{1:0.0f}    D:SAFE".format(globalData.temp, globalData.dtemp)
    else:
        formatString = "{0:0.0f}/{1:0.0f}    D:OPEN".format(globalData.temp, globalData.dtemp)
    if globalData.heat == 1:
        heatString = "A/C "
    elif globalData.heat == 0:
        heatString = "OFF "
    else:
        heatString = "HEAT"
    if globalData.lights == 0:
        lightsString = "OFF"
    else:
        lightsString = "ON"
    format2String = "H:{}   L:{}".format(heatString, lightsString)
    
    display.lcd_display_string(formatString, 1)
    display.lcd_display_string(format2String, 2)
    return
                 
def handle(event = None):  #handler for lights, which is the only functinos that will use threading as 10 seconds is a long time
    sleep(1)
    global lightMsg        #flag to turn on light message in the main loop
    lightMsg = 1
    t = threading.Thread(target=AmbientLightCon)
    t.daemon = True
    t.start()
    sleep(3)
    return

def AmbientLightCon(): #threaded function to turn on lights for 10 seconds
    global globalData
    gpio.output(greenLED, gpio.HIGH)
    globalData.changeLights(1)
    sleep(10)
    gpio.output(greenLED, gpio.LOW)
    globalData.changeLights(0)
    return

def TempRaise(event = None):
    global globalData
    #print(globalData.dtemp)
    if (globalData.dtemp < 85):  #raise temp with upper limit 85
        globalData.changeDtemp(globalData.dtemp + 1)
    return
        
def TempLower(event = None):
    global globalData
    if (globalData.dtemp > 65):  #lower desired temp with lower limit 65
        globalData.changeDtemp(globalData.dtemp - 1)
    return

def DoorwHandler(event = None):
    global doorToggle
    global globalData
    global last
    global doorwMsg
    if doorToggle == 0:        #check if door is already open
        doorToggle = 1
        doorwMsg = 1           
        globalData.changeHeat(0)     #update doorw and heat values in global struct
        globalData.changeDoorw(1)
        gpio.output(redLED, gpio.LOW)  #update leds
        gpio.output(blueLED, gpio.LOW)
    else:
        globalData.changeDoorw(0)
        doorToggle = 0
        last = 0


gpio.setmode(gpio.BCM)
gpio.setwarnings(False)

gpio.setup(tempHigher, gpio.IN, pull_up_down=gpio.PUD_UP)
gpio.setup(tempLower, gpio.IN, pull_up_down=gpio.PUD_UP)
gpio.setup(door, gpio.IN, pull_up_down=gpio.PUD_UP)
gpio.setup(window, gpio.IN, pull_up_down=gpio.PUD_UP)

gpio.setup(5, gpio.IN)            #infrared
gpio.setup(22, gpio.OUT)            #red
gpio.setup(27, gpio.OUT)            #green
gpio.setup(17, gpio.OUT)            #blue light
#gpio.setup(21, gpio.OUT)

gpio.output(17, gpio.LOW)
gpio.output(27, gpio.LOW)
gpio.output(22, gpio.LOW)
display.lcd_display_string("Now booting...", 1)  
display.lcd_display_string("", 2)  
sleep(2)

#event detection for buttons and infrared sensor
gpio.add_event_detect(infraredSensor, gpio.RISING, callback=handle, bouncetime=10000)
gpio.add_event_detect(tempHigher, gpio.FALLING, callback=TempRaise, bouncetime=200)
gpio.add_event_detect(tempLower, gpio.FALLING, callback=TempLower, bouncetime=200)
gpio.add_event_detect(door, gpio.FALLING, callback=DoorwHandler, bouncetime=200)
gpio.add_event_detect(window, gpio.FALLING, callback=DoorwHandler, bouncetime=200)

temp1, temp2, temp3 = 24, 24, 24
h1 = 0
last = 0

while True:
    sleep(0.5)
    humidity, temp = Adafruit_DHT.read(Adafruit_DHT.DHT11, DHT)   #read vals from dht11
    response = requests.get('http://et.water.ca.gov/api/data?appKey=6cc8b450-3c2d-48d9-968d-180f2a278617&targets=211&startDate=2021-06-08&endDate=2021-06-08&dataItems=hly-rel-hum').json()
    cimisHum = response['Data']['Providers'][-1]['Records'][0]['HlyRelHum']['Value']
    #get humidity value from location 211 (closest to my house)
    
    if temp == None:
        temp = temp1                #if temp reads null, use last recorded value
    temp3 = temp2
    temp2 = temp1
    temp1 = temp
    temp = int((temp3+temp2+temp1)/3.0)                #average last 3 measurements
    temp = temp*(9.0/5.0) + 32 + 0.05*float(cimisHum)
    globalData.changeTemp(temp)
    if humidity != None:
        h1 = humidity
    diff = globalData.temp - globalData.dtemp          #check for hvac 
    if (lightMsg == 1): #light flag referenced in light function, if on display message
        lightMsg = 0
        display.lcd_clear()
        display.lcd_display_string("  LIGHTS ON", 1)
        sleep(3)
    if (doorwMsg == 1): #door flag
        doorwMsg = 0
        display.lcd_clear()
        display.lcd_display_string("DOOR/WINDOW OPEN", 1)
        display.lcd_display_string("   HVAC HALTED", 2)
        sleep(3)
        
    if abs(diff) > 3:
        if (diff < 0 and last != 2): #if difference is negative, turn heat, if not, turn ac
            last = 2
            globalData.changeHeat(2)
            display.lcd_clear()
            display.lcd_display_string("   HVAC HEAT", 1)
            gpio.output(redLED, gpio.HIGH)
            gpio.output(blueLED, gpio.LOW)
            sleep(3)
        elif(diff > 0 and last != 1):
            last = 1
            globalData.changeHeat(1)
            gpio.output(blueLED, gpio.HIGH)
            gpio.output(redLED, gpio.LOW)
            display.lcd_clear()
            display.lcd_display_string("   HVAC AC", 1)
            sleep(3)
    else:               # if difference is less than 3, shut off all HVAC
        last = 0
        globalData.changeHeat(0)
        gpio.output(redLED, gpio.LOW)
        gpio.output(blueLED, gpio.LOW)
    #display.lcd_clear()
    displayData()            #display data, update every loop
    