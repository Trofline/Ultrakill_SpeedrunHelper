using UnityEngine;

namespace TroflineMod
{
    public class VisualsEngine : MonoBehaviour
    {
        private Collider[] allColliders;
        private float nextVisualsCheck = 0f;
        private Material lineMaterial;

        void Update()
        {
            bool anyVisuals = MainHandling.showHitboxes || MainHandling.showVolumes || MainHandling.showEnemyHitboxes;

            // FIX 1: unscaledTime ignoriert Slow-Mo/Hitstop. Interval auf 0.5s reduziert für sofortiges Erfassen neuer Spawns!
            if (anyVisuals && Time.unscaledTime > nextVisualsCheck)
            {
                allColliders = FindObjectsOfType<Collider>();
                nextVisualsCheck = Time.unscaledTime + 0.5f;
            }
        }

        void OnRenderObject()
        {
            bool anyVisuals = MainHandling.showHitboxes || MainHandling.showVolumes || MainHandling.showEnemyHitboxes;
            if (!anyVisuals || allColliders == null) return;

            CreateMaterial();
            lineMaterial.SetPass(0);
            Vector3 myPos = Camera.main.transform.position;

            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            GL.Begin(GL.LINES);

            foreach (Collider col in allColliders)
            {
                if (col == null || !col.enabled) continue;

                float dist = Vector3.Distance(myPos, col.transform.position);
                if (dist > 50f) continue;

                // --- GEGNER-CHECK ---
                bool isEnemy = false;
                int layer = col.gameObject.layer;
                if (layer == 10 || layer == 11 || layer == 12) isEnemy = true;
                else if (col.CompareTag("Enemy")) isEnemy = true;
                else if (col.GetComponentInParent<EnemyIdentifier>() != null) isEnemy = true;

                // --- RENDERING ---
                if (isEnemy)
                {
                    if (MainHandling.showEnemyHitboxes)
                    {
                        Color enemyColor = new Color(1f, 0f, 0f, 0.7f);

                        if (col is CapsuleCollider capsule) DrawCapsule(capsule, enemyColor);
                        else if (col is BoxCollider box) DrawOrientedBox(box, enemyColor);
                        else if (col is SphereCollider sphere) DrawSphere(sphere, enemyColor);
                        else DrawBoundingBox(col.bounds, enemyColor);
                    }
                    continue;
                }

                if (col.isTrigger && MainHandling.showVolumes)
                {
                    DrawBoundingBox(col.bounds, new Color(0f, 1f, 0f, 0.6f));
                }
                else if (!col.isTrigger && MainHandling.showHitboxes)
                {
                    DrawBoundingBox(col.bounds, new Color(0f, 1f, 1f, 0.15f));
                }
            }

            GL.End();
            GL.PopMatrix();
        }

        // =====================================================================
        // GEFIXTE ZEICHEN-FUNKTIONEN
        // =====================================================================

        private void DrawOrientedBox(BoxCollider box, Color color)
        {
            GL.Color(color);
            Matrix4x4 localToWorld = box.transform.localToWorldMatrix;
            Vector3 center = box.center;
            Vector3 size = box.size * 0.5f;

            Vector3[] pts = new Vector3[8];
            pts[0] = center + new Vector3(-size.x, -size.y, -size.z);
            pts[1] = center + new Vector3(size.x, -size.y, -size.z);
            pts[2] = center + new Vector3(size.x, -size.y, size.z);
            pts[3] = center + new Vector3(-size.x, -size.y, size.z);
            pts[4] = center + new Vector3(-size.x, size.y, -size.z);
            pts[5] = center + new Vector3(size.x, size.y, -size.z);
            pts[6] = center + new Vector3(size.x, size.y, size.z);
            pts[7] = center + new Vector3(-size.x, size.y, size.z);

            for (int i = 0; i < 8; i++) pts[i] = localToWorld.MultiplyPoint(pts[i]);

            GL.Vertex(pts[0]); GL.Vertex(pts[1]); GL.Vertex(pts[1]); GL.Vertex(pts[2]);
            GL.Vertex(pts[2]); GL.Vertex(pts[3]); GL.Vertex(pts[3]); GL.Vertex(pts[0]);
            GL.Vertex(pts[4]); GL.Vertex(pts[5]); GL.Vertex(pts[5]); GL.Vertex(pts[6]);
            GL.Vertex(pts[6]); GL.Vertex(pts[7]); GL.Vertex(pts[7]); GL.Vertex(pts[4]);
            GL.Vertex(pts[0]); GL.Vertex(pts[4]); GL.Vertex(pts[1]); GL.Vertex(pts[5]);
            GL.Vertex(pts[2]); GL.Vertex(pts[6]); GL.Vertex(pts[3]); GL.Vertex(pts[7]);
        }

        private void DrawCapsule(CapsuleCollider capsule, Color color)
        {
            GL.Color(color);
            Matrix4x4 localToWorld = capsule.transform.localToWorldMatrix;

            float r = capsule.radius;
            float h = capsule.height;
            Vector3 c = capsule.center;

            Vector3 dir = Vector3.up;
            Vector3 right = Vector3.right;
            Vector3 forward = Vector3.forward;

            if (capsule.direction == 0) { dir = Vector3.right; right = Vector3.up; forward = Vector3.forward; }
            if (capsule.direction == 2) { dir = Vector3.forward; right = Vector3.right; forward = Vector3.up; }

            // FIX 2: Punkte im lokalen Raum berechnen, damit sie sich später korrekt mitdrehen!
            Vector3 top = c + dir * Mathf.Max(0, h * 0.5f - r);
            Vector3 bot = c - dir * Mathf.Max(0, h * 0.5f - r);

            Vector3[] p = new Vector3[8];
            p[0] = top + right * r; p[1] = bot + right * r;
            p[2] = top - right * r; p[3] = bot - right * r;
            p[4] = top + forward * r; p[5] = bot + forward * r;
            p[6] = top - forward * r; p[7] = bot - forward * r;

            // In Weltpunkte umwandeln und zeichnen
            for (int i = 0; i < 8; i += 2)
            {
                GL.Vertex(localToWorld.MultiplyPoint(p[i]));
                GL.Vertex(localToWorld.MultiplyPoint(p[i + 1]));
            }
        }

        private void DrawSphere(SphereCollider sphere, Color color)
        {
            GL.Color(color);
            Vector3 center = sphere.transform.TransformPoint(sphere.center);
            float r = sphere.radius * Mathf.Max(sphere.transform.lossyScale.x, sphere.transform.lossyScale.y, sphere.transform.lossyScale.z);

            DrawCircle(center, Vector3.up, r);
            DrawCircle(center, Vector3.right, r);
            DrawCircle(center, Vector3.forward, r);
        }

        private void DrawCircle(Vector3 center, Vector3 normal, float radius)
        {
            Vector3 forward = Vector3.Slerp(normal, -normal, 0.5f);
            Vector3 right = Vector3.Cross(normal, forward).normalized * radius;
            forward = Vector3.Cross(normal, right).normalized * radius;

            for (int i = 0; i < 16; i++)
            {
                float angle = (float)i / 16f * Mathf.PI * 2f;
                float nextAngle = (float)(i + 1) / 16f * Mathf.PI * 2f;

                GL.Vertex(center + right * Mathf.Cos(angle) + forward * Mathf.Sin(angle));
                GL.Vertex(center + right * Mathf.Cos(nextAngle) + forward * Mathf.Sin(nextAngle));
            }
        }

        private void DrawBoundingBox(Bounds b, Color color)
        {
            GL.Color(color);
            Vector3 c = b.center;
            Vector3 e = b.extents;

            Vector3 p0 = c + new Vector3(-e.x, -e.y, -e.z);
            Vector3 p1 = c + new Vector3(e.x, -e.y, -e.z);
            Vector3 p2 = c + new Vector3(e.x, -e.y, e.z);
            Vector3 p3 = c + new Vector3(-e.x, -e.y, e.z);
            Vector3 p4 = c + new Vector3(-e.x, e.y, -e.z);
            Vector3 p5 = c + new Vector3(e.x, e.y, -e.z);
            Vector3 p6 = c + new Vector3(e.x, e.y, e.z);
            Vector3 p7 = c + new Vector3(-e.x, e.y, e.z);

            GL.Vertex(p0); GL.Vertex(p1); GL.Vertex(p1); GL.Vertex(p2);
            GL.Vertex(p2); GL.Vertex(p3); GL.Vertex(p3); GL.Vertex(p0);
            GL.Vertex(p4); GL.Vertex(p5); GL.Vertex(p5); GL.Vertex(p6);
            GL.Vertex(p6); GL.Vertex(p7); GL.Vertex(p7); GL.Vertex(p4);
            GL.Vertex(p0); GL.Vertex(p4); GL.Vertex(p1); GL.Vertex(p5);
            GL.Vertex(p2); GL.Vertex(p6); GL.Vertex(p3); GL.Vertex(p7);
        }

        private void CreateMaterial()
        {
            if (!lineMaterial)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                lineMaterial.SetInt("_ZTest", 4);
            }
        }
    }
}