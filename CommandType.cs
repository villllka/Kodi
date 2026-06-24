public enum CommandType
{
    None = 0,

    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,

    PlaceBlock,

    If,
    ElseIf,
    Else,
    EndIf,

    While,
    For,
    EndLoop,

    Then,

    And,
    Or,
    Not,

    Equal,
    Danger,

    True,
    False
}