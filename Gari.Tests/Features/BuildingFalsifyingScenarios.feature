Feature: Building Falsifying Scenarios

Background:
    Given SUT is expected to obey the following rules
    | Rule                                             | Parsed Form (only for convinience)             |
    | Нажатие левой кнопки должно включать лампу       | Нажать(Левый(кнопка)) ⊃> Включить(лампа)       |
    | Нажатие правой кнопки должно выключать лампу     | Нажать(Правый(кнопка)) ⊃> Выключить(лампа)     |
    | Если лампа включена, то левая кнопка неактивна   | ВКЛЮЧЕННЫЙ(лампа) ⇒ НЕАКТИВНЫЙ(Левый(кнопка))  |
    | Если лампа выключена, то правая кнопка неактивна | ВЫЛЮЧЕННЫЙ(лампа) ⇒ НЕАКТИВНЫЙ(Правый(кнопка)) |


Scenario: Checking direct consequences of all actions according to the rules
    Then Falsifying Scenario should be generated
    | User action            | Expected Outcome   |
    | Нажать(Левый(Кнопка))  | ВКЛЮЧЕННЫЙ(Лампа)  |
    | Нажать(Правый(Кнопка)) | ВЫКЛЮЧЕННЫЙ(Лампа) |
