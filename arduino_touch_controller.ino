// Firmware of arudino touch controller

// a pair of pins for a panel
struct Pin {
  uint8_t output; // a pin that supplies current
  uint8_t input; // a pin that is connected to the panel and reads the voltage
};

// represents the state of the panel being touched or not.
enum PanelState {
  UNTOUCHED,
  TOUCHED
};

// a set of variables about a panel
struct Panel {
  Pin pin; // pins that are connected to the panel
  uint32_t high_voltage_count; // the number of times high voltage was read from pin.output
  enum PanelState state; // the panel state (touched or not)
};

// the number of panels
const uint8_t NUM_PIN_PAIR = 8;

// definition of pin pairs for each panel
Panel panel_list[NUM_PIN_PAIR] = {
  {{2, 3}},  {{19, 4}},  {{18, 5}},  {{17, 6}},  {{16, 7}},  {{15, 8}},  {{14, 9}},  {{11, 10}}
};

// how many times the low voltage must be read to enter the UNTOUCHED state
const uint8_t THRESHOLD_COUNT_UNTOUCH = 5;

// the interval time between when the output pin raises its voltage and when the input pin reads it
const uint8_t CHARGE_TIME_MICRO_SECOND = 40;

void setup() {
  // Set the I/O mode for each pin
  for (uint8_t i = 0; i < NUM_PIN_PAIR; i++) {
    pinMode(panel_list[i].pin.output, OUTPUT);
    pinMode(panel_list[i].pin.input, INPUT);
  }
  // Set the initial states
  for (uint8_t i = 0; i < NUM_PIN_PAIR; i++) {
    panel_list[i].high_voltage_count = THRESHOLD_COUNT_UNTOUCH;
    panel_list[i].state = UNTOUCHED;
  }
  // Establish a connection with the PC
  Serial.begin(115200);
}

// Detect the voltage rise delay of the panel
uint8_t getVoltageInput(Panel *panel) {
  uint8_t output;

  // Start supplying current from the output pin
  digitalWrite(panel->pin.output, HIGH);
  // Wait for the voltage on the input pins to rise
  delayMicroseconds(CHARGE_TIME_MICRO_SECOND);
  // Read the voltage on the input pin
  output = digitalRead(panel->pin.input);
  // Stop supplying the current
  digitalWrite(panel->pin.output, LOW);

  return output;
}

// Update high_voltage_count of the panel
void updatePanelHighVoltageCount(Panel *panel) {
  if (getVoltageInput(panel) == HIGH) {
    panel->high_voltage_count++;
  } else {
    panel->high_voltage_count = 0;    
  }
}

// Update the state of the panel
void updatePanelState(Panel *panel) {
  if (panel->high_voltage_count >= THRESHOLD_COUNT_UNTOUCH) {
    // If HIGH voltage is read (i.e. the voltage rise delay was not observed)
    // THRESHOLD_COUNT_UNTOUCH times or more consecutively, set to the UNTOUCHED state.
    panel->state = UNTOUCHED;
  } else {
    // Otherwise, set to the TOUCHED state.
    panel->state = TOUCHED;
  }
}

// Send all panel status via the serial port
void sendPanelStateList(Panel *panel) {
  // Here, we represent the state of a panel as a single bit
  // and pack all panel states into a single byte.
  uint8_t data = 0;

  for (uint8_t i = 0; i < NUM_PIN_PAIR; i++) {
    // a bit shift
    data <<= 1;
    // write the state
    if (panel_list[i].state == TOUCHED) {
      data |= 1;
    }
  }
  // Send the byte data via the serial port
  Serial.write(data);
}

// main loop
void loop() {
  // Update the state of each panel
  for (uint8_t i = 0; i < NUM_PIN_PAIR; i++) {
    Panel *panel = &panel_list[i];
    updatePanelHighVoltageCount(panel);
    updatePanelState(panel);
  }
  // Send the state to PC
  sendPanelStateList(panel_list);
}
