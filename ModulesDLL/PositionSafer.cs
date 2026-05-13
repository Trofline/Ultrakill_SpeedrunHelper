using UnityEngine;

namespace TroflineMod
{
    public class PositionSafer : MonoBehaviour
    {
        private struct SaveSlot
        {
            public Vector3 pos;
            public Vector3 vel;
            public Quaternion rot;
            public bool hasData;
        }

        private static SaveSlot[] slots = new SaveSlot[4];

        public static void Save(int index)
        {
            var nm = NewMovement.Instance; // Nutzt Ultrakills Singleton
            if (nm == null) return;

            slots[index].pos = nm.transform.position;
            slots[index].rot = nm.transform.rotation;
            slots[index].vel = nm.rb.velocity;
            slots[index].hasData = true;
        }

        public static void Teleport(int index)
        {
            if (!slots[index].hasData) return;
            var nm = NewMovement.Instance;
            if (nm == null) return;

            nm.transform.position = slots[index].pos;
            nm.transform.rotation = slots[index].rot;
            nm.rb.velocity = slots[index].vel;
        }
    }
}