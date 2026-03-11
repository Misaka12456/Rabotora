namespace RabotoraX.Core.Inputs;

/// <summary>
/// Represents a keyboard key code for RaboInput.
/// </summary>
public enum KeyCode
{
    // --- Basic Control Keys ---
    None = 0,
    Backspace = 8,
    Back = Backspace,            // Alias for Backspace
    Tab = 9,
    Enter = 13,
    Return = Enter,         // Alias for Enter
    Shift = 16,          // General Shift
    Control = 17,        // General Control
    Alt = 18,            // General Alt
    Pause = 19,
    CapsLock = 20,
    Escape = 27,
    Space = 32,
    PageUp = 33,
    PageDown = 34,
    End = 35,
    Home = 36,
    LeftArrow = 37,
    UpArrow = 38,
    RightArrow = 39,
    DownArrow = 40,
    PrintScreen = 44,
    Insert = 45,
    Delete = 46,

    // --- Main Keyboard (Numzone) ---
    D0 = 48, D1, D2, D3, D4, D5, D6, D7, D8, D9,

    // --- Alphabet Keys ---
    A = 65, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,

    // --- System & Modifier Keys ---
    LWin = 91,
    RWin = 92,
    Menu = 93,           // Application / Context Menu key
    Sleep = 95,
    
    LShiftKey = 160,
    RShiftKey = 161,
    LControlKey = 162,
    RControlKey = 163,
    LAltKey = 164,
    RAltKey = 165,

    // --- Small Keyboard (Numpad) ---
    Numpad0 = 96, Numpad1, Numpad2, Numpad3, Numpad4, Numpad5, Numpad6, Numpad7, Numpad8, Numpad9,
    Multiply = 106,      // Numpad *
    Add = 107,           // Numpad +
    Separator = 108,     // Numpad Enter/Separator
    Subtract = 109,      // Numpad -
    Decimal = 110,       // Numpad .
    Divide = 111,        // Numpad /

    // --- Function (Fn) Keys ---
    F1 = 112, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
    F13, F14, F15, F16, F17, F18, F19, F20, F21, F22, F23, F24,

    // --- Lock Keys ---
    NumLock = 144,
    ScrollLock = 145,

    // --- Browser & System Control Keys ---
    BrowserBack = 166,
    BrowserForward = 167,
    BrowserRefresh = 168,
    BrowserStop = 169,
    BrowserSearch = 170,
    BrowserFavorites = 171,
    BrowserHome = 172,
    LaunchMail = 180,

    // --- Media & Volume Control Keys ---
    VolumeMute = 173,
    VolumeDown = 174,
    VolumeUp = 175,
    MediaNextTrack = 176,
    MediaPrevTrack = 177,
    MediaStop = 178,
    MediaPlayPause = 179,

    // --- Symbol (OEM) Keys ---
    Semicolon = 186,     // ;
    EqualSign = 187,     // =
    Comma = 188,         // ,
    Dash = 189,          // -
    Minus = Dash,         // Alias for Dash
    Period = 190,        // .
    ForwardSlash = 191,  // /
    Tilde = 192,         // ~
    Grave = Tilde,         // Alias for Tilde (`), commonly used for console
    LeftBracket = 219,   // [
    Backslash = 220,     // \
    RightBracket = 221,  // ]
    Quote = 222          // '
}