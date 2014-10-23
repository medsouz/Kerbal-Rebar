using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KerbalRebar
{
	[KSPAddonFixed(KSPAddon.Startup.MainMenu, true, typeof(KerbalRebar))]
    public class KerbalRebar : MonoBehaviour
    {
		public string installDir = AssemblyLoader.loadedAssemblies.GetPathByType(typeof(KerbalRebar));

		//Settings
		[KSPField] public float damageDecayMultiplier = 1;
		[KSPField] public float impactMomentumMultiplier = 1;
		[KSPField] public float repairCostMultiplier = 1;
		[KSPField] public float collapseDurationMultiplier = 1;
		[KSPField] public Vector3 collapseOffsetAddend = Vector3.zero;
		[KSPField] public Vector3 collapseTiltAddend = Vector3.zero;

	    void Awake()
	    {
		    loadConfig();
			saveConfig(); //Save config incase new values were added since the config was initially created.

			Debug.Log("Rebar - Modifying buildings...");

			foreach (KeyValuePair<string, ScenarioDestructibles.ProtoDestructible> des in ScenarioDestructibles.protoDestructibles)
			{
				DestructibleBuilding dB = des.Value.dBuildingRefs[0];
				dB.damageDecay *= damageDecayMultiplier;
				dB.impactMomentumThreshold *= impactMomentumMultiplier;
				dB.RepairCost *= repairCostMultiplier;
				foreach (DestructibleBuilding.CollapsibleObject cO in dB.CollapsibleObjects)
				{
					cO.collapseDuration *= collapseDurationMultiplier;
					cO.collapseOffset += collapseOffsetAddend;
					cO.collapseTiltMax += collapseTiltAddend;
				}
			}

			Debug.Log("Rebar - Done!");
	    }

		public bool loadConfig()
		{
			ConfigNode cfg = ConfigNode.Load(installDir + @"\KerbalRebar.cfg".Replace('/', '\\'));
			if (cfg != null)
			{
				foreach (FieldInfo f in GetType().GetFields())
				{
					if (Attribute.IsDefined(f, typeof(KSPField)))
					{
						if (cfg.HasValue(f.Name))
						{
							object value;
							if (f.FieldType == typeof (Vector3))
							{
								value = ConfigNode.ParseVector3(cfg.GetValue(f.Name));
							}
							else
							{
								value = Convert.ChangeType(cfg.GetValue(f.Name), f.FieldType);
							}
							f.SetValue(this, value);
						}
					}
				}
				return true;
			}
			return false;
		}

		public void saveConfig()
		{
			ConfigNode cfg = new ConfigNode();
			foreach (FieldInfo f in GetType().GetFields())
			{
				if (Attribute.IsDefined(f, typeof(KSPField)))
				{
					object value;
					if (f.FieldType == typeof (Vector3))
					{
						value = ConfigNode.WriteVector((Vector3)f.GetValue(this));
					}
					else
					{
						value = f.GetValue(this);
					}
					cfg.AddValue(f.Name, value);
				}
			}
			Directory.CreateDirectory(installDir);
			cfg.Save(installDir + "/KerbalRebar.cfg", "Kerbal Rebar - https://github.com/medsouz/Kerbal-Rebar");
		}
    }
}
