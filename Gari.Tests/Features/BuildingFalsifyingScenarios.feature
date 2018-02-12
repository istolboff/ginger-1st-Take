Feature: Building Falsifying Scenarios

Background:
    Given SUT is expected to obey the following rules
    | Rule                                             | Parsed Form                                  |
    | Нажатие левой кнопки должно включать лампу       | Нажать(Левая(кнопка)) ⊃> Включенный(лампа)   |
    | Нажатие правой кнопки должно выключать лампу     | Нажать(Правая(кнопка)) ⊃> Выключенный(лампа) |
    | Если лампа включена, то левая кнопка неактивна   | ВКЛЮЧЕН(лампа) ⇒ НЕАКТИВНЫЙ(Левая(кнопка))   |
    | Если лампа выключена, то правая кнопка неактивна | ВЫЛЮЧЕН(лампа) ⇒ НЕАКТИВНЫЙ(Правая(кнопка))  |


Scenario: Checking direct consequences of all actions according to the rules
    Then Falsifying Scenario should be generated
    | User action          | Expected Outcome   |
    | Нажать левую кнопку  | Включенный(лампа)  |
    | Нажать правую кнопку | Выключенный(лампа) |
