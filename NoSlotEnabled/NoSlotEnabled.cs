using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using FrooxEngine;
using FrooxEngine.UIX;

using HarmonyLib;

using ResoniteModLoader;

namespace NoSlotEnabled;

public class NoSlotEnabled : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.0";
	public override string Name => "NoSlotEnabled";
	public override string Author => "Delta";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/XDelta/NoSlotEnabled/";

	public override void OnEngineInit() {
		Harmony harmony = new("net.deltawolf.NoSlotEnabled");
		harmony.PatchAll();
	}

	[HarmonyPatch(typeof(SlotInspector), "OnChanges")]
	public static class SlotInspector_OnChanges_Transpiler {
		//Removes `ui.BooleanMemberEditor(this._rootSlot.Target.ActiveSelf_Field, null);` in SlotInspector.OnChanges()
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var codes = instructions.ToList();

			var booleanEditor = AccessTools.Method(
				typeof(UIBuilderEditors),
				nameof(UIBuilderEditors.BooleanMemberEditor),
				[typeof(UIBuilder), typeof(IField), typeof(string)]);

			for (int i = 0; i < codes.Count - 7; i++) {
				if (codes[i + 2].opcode == OpCodes.Ldfld && codes[i + 3].opcode == OpCodes.Callvirt &&
					codes[i + 4].opcode == OpCodes.Ldfld && codes[i + 5].opcode == OpCodes.Ldnull &&
					(codes[i + 6].opcode == OpCodes.Call || codes[i + 6].opcode == OpCodes.Callvirt) &&
					codes[i + 6].operand is MethodInfo method && method == booleanEditor) {
					
					for (int j = 0; j <= 6; j++)
						codes[i + j].opcode = OpCodes.Nop;

					// NOP the pop
					if (i + 7 < codes.Count && codes[i + 7].opcode == OpCodes.Pop)
						codes[i + 7].opcode = OpCodes.Nop;
				}
			}
			return codes.AsEnumerable();
		}
	}
}
