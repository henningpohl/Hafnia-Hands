using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Globalization;
using System.IO;

public class ExtractHandMesh : MonoBehaviour {

    private bool gotLeftHand = false;
    private bool gotRightHand = false;
    private bool done = false;

    public OVRPlugin.Mesh leftHandMesh;
    public OVRPlugin.Mesh rightHandMesh;

    void Start() {
        leftHandMesh = new OVRPlugin.Mesh();
        rightHandMesh = new OVRPlugin.Mesh();
    }

    // Update is called once per frame
    void Update() {
        if(done) {
            return;
        }

        if(gotLeftHand == false) {
            gotLeftHand = OVRPlugin.GetMesh(OVRPlugin.MeshType.HandLeft, out leftHandMesh);
        }
        if(gotRightHand == false) {
            gotRightHand = OVRPlugin.GetMesh(OVRPlugin.MeshType.HandRight, out rightHandMesh);
        }

        if(gotLeftHand && gotRightHand) {

            //MeshToFbx(leftHandMesh);
            MeshToObj(leftHandMesh, "C:\\Users\\henni\\Desktop\\LeftHand.obj");
            MeshToObj(rightHandMesh, "C:\\Users\\henni\\Desktop\\RightHand.obj");
            done = true;

        }
    }


    void MeshToObj(OVRPlugin.Mesh mesh, string filename) {
        CultureInfo culture = CultureInfo.InvariantCulture;
        StringBuilder sb = new StringBuilder();

        for(var i = 0; i < mesh.NumVertices; ++i) {
            sb.AppendFormat(culture, "v {0:F5} {1:F5} {2:F5}\n", mesh.VertexPositions[i].x, mesh.VertexPositions[i].y, mesh.VertexPositions[i].z);
        }
        for(var i = 0; i < mesh.NumVertices; ++i) {
            sb.AppendFormat(culture, "vt {0:F5} {1:F5}\n", mesh.VertexUV0[i].x, mesh.VertexUV0[i].y);
        }
        for(var i = 0; i < mesh.NumVertices; ++i) {
            sb.AppendFormat(culture, "vn {0:F5} {1:F5} {2:F5}\n", mesh.VertexNormals[i].x, mesh.VertexNormals[i].y, mesh.VertexNormals[i].z);
        }

        for(var i = 0; i < mesh.NumIndices; i += 3) {
            sb.AppendFormat(culture, "f {0:D}/{0:D}/{0:D} {1:D}/{1:D}/{1:D} {2:D}/{2:D}/{2:D}\n", mesh.Indices[i + 0] + 1, mesh.Indices[i + 1] + 1, mesh.Indices[i + 2] + 1);
        }

        using(var fs = File.OpenWrite(filename)) {
            var data = new UTF8Encoding(true).GetBytes(sb.ToString());
            fs.Write(data, 0, data.Length);
        }
    }

    /*
    void MeshToFbx(OVRPlugin.Mesh mesh) { 
        var x = new UnityGLTF.GLTFSceneExporter(new Transform[] { null });
        x.SaveGLTFandBin("c:/", "test");


    }
    */
}
