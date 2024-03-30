using Luminance.Core.Hooking;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;

namespace NoxusBoss.Core.Fixes
{
    /* CONTEXT: 
     * Screen shaders (and by extension, this library's filters) were not built for what they have become. Terraria really only uses them for tinting effects, such as pillar color overlays.
     * Things like screen distortions and sophisticated post-processing shader draws are foreign to it.
     * As such, the vanilla game works under the assumption that it's acceptable to disallow screen shaders on Retro and Trippy lighting modes.
     * Unfortunately, this is far from true when used more broadly in the way that many mods use screen shaders.
     * 
     * As such, this Retro/Trippy "no screen shaders" behavior is completely removed via this IL edit until further notice.
     */
    public sealed class ScreenShaderFixer : ILEditProvider
    {
        public override void PerformEdit(ILContext il, ManagedILEdit edit)
        {
            ILCursor cursor = new(il);
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt<Lighting>("get_NotRetro")))
            {
                edit.LogFailure("The Lighting.NotRetro property could not be found.");
                return;
            }

            // Emit OR 1 on the "can screen shaders be drawn" bool to make it always true, regardless of lighting mode.
            cursor.Emit(OpCodes.Ldc_I4_1);
            cursor.Emit(OpCodes.Or);
        }

        public override void Subscribe(ManagedILEdit edit) => IL_Main.DoDraw += edit.SubscriptionWrapper;

        public override void Unsubscribe(ManagedILEdit edit) => IL_Main.DoDraw -= edit.SubscriptionWrapper;
    }
}
