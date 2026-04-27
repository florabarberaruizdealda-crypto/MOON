// ESP32/MOVEMENTosc.ino
// Configuración de comunicación y efectos integrados para ESP32

// ================================ CONFIGURACIÓN ================================
#include <WiFi.h>
#include <WiFiUdp.h>
#include <OSCMessage.h>
#include <Wire.h>
#include <FastLED.h>

// === CONFIGURACIÓN DE REDES ===
const char* ssid = "GL-AR300M-7ac-NOR";
const char* pass = "";

// === CONFIGURACIÓN DE OSC / Udp ===
WiFiUDP Udp;
const IPAddress outIp(192, 168, 8, 199);   // IP del ordenador de Unity
const unsigned int outPort = 9999;         // Puerto de salida para Unity
const unsigned int localPort = 8888;       // Puerto local para escuchar

// === CONFIGURACIÓN DE LEDS (WS2816, FastLED) ===
#define NUM_LEDS 32
#define DATA_PIN 32                        // PIN GPIO 32 (RGB WS2816)

// Definir el array de leds
CRGB leds[NUM_LEDS];

// Orden de colores: GRB (común para WS2816/Neopixels)
// Si tu tira es RGB, cambia el orden o usa FastLED.addLeds<WS2816, DATA_PIN, GRB>
// Para GRB, no especificas el orden explícitamente, pero se recomienda:
// FastLED.addLeds<WS2816, DATA_PIN, GRB>(leds, NUM_LEDS);

// === CONFIGURACIÓN DE TOQUES CAPACITIVOS ===
// Ajustar estos pines según tu placa de sensores. Los toques capacitivos nativos del ESP32
// suelen estar en GPIOs como 35, 36, 37, 38, 39 o 33, 34, 45.
// Asegúrate de usar una librería compatible como TouchESP32 o la que ya tienes instalada.
const int touchLeft = 44;    // Ejemplo, ajusta según tu placa
const int touchRight = 45;
const int touchUp = 46;
const int touchDown = 47;
const int touchThreshold = 50;

// === CONFIGURACIÓN DE DRIVER HÁPTICO (DRV2605, I2C) ===
#include "TouchSensorDriver.h"  // o la librería que uses
// Si no tienes la librería TouchSensorDriver, usa tu propia librería o DRV2605Lib
// Aquí asumo que ya tienes la librería en tu entorno (especial si ya tienes el sketch funcionando)

// === VARIABLES PARA EFECTOS LED ===
static uint16_t sPseudotime = 0;
static uint16_t sLastMillis = 0;
static uint16_t sHue16 = 0;
uint8_t sat8 = beatsin88( 87, 220, 250);
uint8_t brightdepth = beatsin88( 341, 96, 224);
uint16_t brightnessthetainc16 = beatsin88( 203, (25 * 256), (40 * 256));
uint8_t msmultiplier = beatsin88(147, 23, 60);
uint16_t hue16 = 0;
uint16_t hueinc16 = beatsin88(113, 1, 3000);
uint16_t ms = millis();
uint16_t deltams = ms - sLastMillis;

// === VARIABLES DE DRIVER ===
DRV2605 driver;
void (*drv_waveform_func)(void) = nullptr;  // Función de onda de audio (si usas driver por audio)
// drv.setWaveform(0, effect[1]); // Función opcional según tu caso

// === INICIALIZACIÓN DE DRIVER (si aplica en tu librería) ===
TouchSensorDriver drv;  // o DRV2605 drv;

// === VARIABLES DE DECISIÓN / LÓGICA ===
int currentDecision = 0;
unsigned long lastDecisionTime = 0;
const unsigned long decisionCooldown = 500; // Tiempo para no repetir decisión en loop

// ================================ SETUP ================================
void setup() {
  // Iniciar Serial
  Serial.begin(115200);

  // Iniciar WiFi y UDP
  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, pass);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.println("Conectando WiFi...");
  }
  Serial.println("WiFi conectado");

  // Iniciar conexión UDP
  Udp.begin(localPort);

  // Iniciar I2C
  Wire.begin();

  // Iniciar Driver háptico
  drv.begin();
  drv.selectLibrary(1); // Librería ERM (no LRA)
  drv.setMode(DRV2605_MODE_INTTRIG);
  
  // Inicializar FastLED (WS2816 con pin GRB)
  // Si tu tira es RGB, usa RGB en lugar de GRB en la segunda línea
  FastLED.addLeds<WS2816, DATA_PIN, GRB>(leds, NUM_LEDS); // GRB es GRB (GRB = Green Red Blue)

  // Inicializar efectos de driver (opcional)
  // drv.setWaveform(0, effect[1]); 

  Serial.println("Setup complete");
}

// ================================ LOOP ================================
void loop() {

  // === LÓGICA DE LEVEMENTO Y CONTROL DE LEDS ===
  // Actualizamos los LEDs y el driver
  if (millis() - lastDecisionTime > decisionCooldown) {
    // Si no ha pasado el cooldown, verificamos la última decisión recibida
    currentDecision = getDecisionFromUnity(); // O leer de mensaje previo
    lastDecisionTime = millis();
    applyDecision(currentDecision);
  }

  // === EFECTO DE LED: Galería FastLED (Bucle de animación) ===
  static uint8_t frameCount = 0;
  frameCount++;
  updateLEDFrame(frameCount);

  // === TOCATE: LECTURA DE SENSORES TÁCTILES ===
  if (touchRead(touchLeft) < touchThreshold) {
    OSCMessage msg("/moon/move");
    msg.add(0);
    sendOSCMessage(msg);
    Serial.println("LEFT");
    delay(100);
  }
  else if (touchRead(touchRight) < touchThreshold) {
    OSCMessage msg("/moon/move");
    msg.add(1);
    sendOSCMessage(msg);
    Serial.println("RIGHT");
    delay(100);
  }
  else if (touchRead(touchUp) < touchThreshold) {
    OSCMessage msg("/moon/move");
    msg.add(2);
    sendOSCMessage(msg);
    Serial.println("UP");
    delay(100);
  }
  else if (touchRead(touchDown) < touchThreshold) {
    OSCMessage msg("/moon/move");
    msg.add(3);
    sendOSCMessage(msg);
    Serial.println("DOWN");
    delay(100);
  }

  // === ESCUCHAR MENSAJES DE UNITY ===
  if (Udp.parsePacket()) {
    OSCMessage msg("/moon/decision");
    if (msg.hasArg()) {
      msg.getArgAsInt(0, currentDecision); // Leer decisión
      lastDecisionTime = millis();
    }
  }

  // === EJECUTAR EFECTOS DE DRIVER ===
  // drv.go();  // Ejecutar driver en modo go
  // drv.play(); // Si tu librería usa .play()

  // === ACTUALIZAR Y MOSTRAR LEDS ===
  FastLED.show();
  delay(100); // Evita saturación, ajusta si es necesario
}

// ================================ FUNCIONES AUXILIARES ================================

// === FUNCION: RECIBIR DECISIÓN (De Unity) ===
int getDecisionFromUnity() {
  // Si no se ha recibido una decisión por UDP, devuelve 0
  // Aquí puedes agregar lógica para mantener la última decisión
  // o leer de una estructura de datos
  return 0;
}

// === FUNCION: ENVIAR MENSAJE OSC ===
void sendOSCMessage(OSCMessage& msg) {
  if (Udp.beginPacket(outIp, outPort)) {
    msg.send(Udp);
    Udp.endPacket();
    msg.empty();
  }
}

// === FUNCION: APLICAR DECISIÓN ===
void applyDecision(int decision) {
  switch (decision) {
    case 1:
      Serial.println("DECISION 1: Efecto 1 activado");
      drv.setWaveform(0, effect[1]); // O tu función de efecto 1
      drv.go(); // Ejecutar efecto
      leds[0] = CRGB::Red;            // Ejemplo de efecto
      break;
    case 2:
      Serial.println("DECISION 2: Efecto 2 activado");
      drv.setWaveform(0, effect[2]); // Otu función de efecto 2
      drv.go();
      leds[0] = CRGB::Green;
      break;
    case 3:
      Serial.println("DECISION 3: Efecto 3 activado");
      drv.setWaveform(0, effect[3]); // Otu función de efecto 3
      drv.go();
      leds[0] = CRGB::Blue;
      break;
    case 0: // Default
      Serial.println("DECISION 0: Efecto por defecto");
      drv.setWaveform(0, effect[0]); // Efecto por defecto
      drv.go();
      leds[0] = CRGB::White;
      break;
    default:
      Serial.println("DECISION DEFAULT: Acción desconocida");
  }
}

// === FUNCION: ACTUALIZAR EFECTO DE LED (Ejemplo de animación) ===
void updateLEDFrame(uint8_t frameCount) {
  // Actualizar variables de tiempo
  uint16_t ms = millis();
  uint16_t deltams = ms - sLastMillis;
  sLastMillis = ms;

  // Actualizar valores de pseudotiempos
  sPseudotime += deltams * msmultiplier;
  sHue16 += deltams * beatsin88(400, 5, 9);
  uint16_t brightnesstheta16 = sPseudotime;

  // Bucle para renderizar leds
  for (uint16_t i = 0; i < NUM_LEDS; i++) {
    hue16 += hueinc16;
    uint8_t hue8 = hue16 / 256;
    brightnesstheta16 += brightnessthetainc16;
    uint16_t b16 = sin16(brightnesstheta16) + 32768;
    uint16_t bri16 = (uint32_t)((uint32_t)b16 * (uint32_t)b16) / 65536;
    uint8_t bri8 = (uint32_t)(((uint32_t)bri16) * brightdepth) / 65536;
    bri8 += (255 - brightdepth);
    CRGB newcolor = CHSV(hue8, sat8, bri8);
    
    // Invertimos para que la animación sea desde la última parte del strip
    uint16_t pixelnumber = i;
    pixelnumber = (NUM_LEDS - 1) - pixelnumber;
    
    nblend(leds[pixelnumber], newcolor, 64);
  }
}
