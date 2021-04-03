using MonsterFlow;

/// <summary>
///    Single point of contact class for input configuration.
/// </summary>
public static class ControlsManager
{
	private static Controls inputs;

	// ReSharper disable once ConvertToNullCoalescingCompoundAssignment
	/// <summary> Global accessor for input configuration object. </summary>
	public static Controls Inputs => inputs ?? (inputs = new Controls());
}