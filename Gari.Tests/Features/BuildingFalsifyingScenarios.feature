Feature: Building Falsifying Scenarios

Background:
    Given SUT is expected to obey the following rules
    | Rule                                                                    | Parsed Form                                                                |
    | Нажатие кнопки 'Втолкнуть' должно добавлять в начало стека строку 00001 | Event(Кнопка('Втолкнуть'), Нажат) ⇒ ДОБАВИТЬ(Строка('00001'), Начало(стек)) |
    | Нажатие кнопки 'Вытолкнуть' должно удалять строку из начала стека       | Event(Кнопка('Вытолкнуть'), Нажат) ⇒ УДАЛИТЬ(Строка(), Начало(стек))      |
    | Нажатие кнопки 'Очистить' должно удалять из стека все строки            | Event(Кнопка('Очистить'), Нажат) ⇒ (∀ s) s ∈ стек ⇒ УДАЛИТЬ(s, стек)    |
    | Если стек пуст, то кнопка 'Вытолкнуть' должна быть неактивна            | ПУСТОЙ(стек) ⇒ ¬АКТИВНЫЙ(Кнопка('Вытолкнуть'))                            |

Scenario: Bringing SUT to a desired state in order to check that SUT's behavior is correct when switching to that state
    Then Falsifying Scenario should be generated
    | Scenario Steps                               |
    | Нажать кнопку 'Очистить'                     |
    | Нажать кнопку 'Втолкнуть'                    |
    | Нажать кнопку 'Вытолкнуть'                   |
    | Проверить, что кнопка 'Вытолкнуть' неактивна |

Scenario: Bringing SUT to a desired state, using different routs, in order to check that SUT's behavior in that state is correct regardless of the route it arrived there
    Then Falsifying Scenario should be generated
    | Scenario Steps                               |
    | Нажать кнопку 'Очистить'                     |
    | Нажать кнопку 'Втолкнуть'                    |
    | Нажать кнопку 'Вытолкнуть'                   |
    | Проверить, что кнопка 'Вытолкнуть' неактивна |
    And Falsifying Scenario should be generated
    | Scenario Steps                               |
    | Нажать кнопку 'Очистить'                     |
    | Нажать кнопку 'Втолкнуть'                    |
    | Нажать кнопку 'Очистить'                     |
    | Проверить, что кнопка 'Вытолкнуть' неактивна |

Scenario: When there are no todos, #main and #footer should be hidden
    Then falsifying statement should be generated
    | Rule Text                                                     | Falsifying Statement                                     |
    | Если список дел пуст, то #main и #footer должны быть невидимы | ПУСТОЙ(список-дел) & (ВИДИМЫЙ(#main) | ВИДИМЫЙ(#footer)) |

