using UnityEngine;
using System.Collections;

///<summary>
///Draws skidmarks on the terrain or any other object with a collider
///</summary>
public class SkidmarksManager : MonoBehaviour {
			
	public int maxMarks = 1024;			
	public float markWidth = 0.275f;		
	public float groundOffset = 0.02f;	
	public float minDistance = 0.1f;	
	
	private int indexShift;
	private int numMarks = 0;
	
	class markSection{
		public Vector3 pos = Vector3.zero;
		public Vector3 normal = Vector3.zero;
		public Vector4 tangent = Vector4.zero;
		public Vector3 posl = Vector3.zero;
		public Vector3 posr = Vector3.zero;
		public float intensity = 0.0f;
		public int lastIndex = 0;
	};
	
	private markSection[] skidmarks;
	
	private bool  updated = false;
	
	void  Start (){
		if (transform.position != Vector3.zero)
			transform.position = Vector3.zero;
	}
	
	void  Awake (){
		skidmarks = new markSection[maxMarks];
		for(int i= 0; i < maxMarks; i++)
			skidmarks[i]=new markSection();
		if(GetComponent<MeshFilter>().mesh == null)
			GetComponent<MeshFilter>().mesh = new Mesh();
	}

	///<summary>
	///Function called by the tyres to add a skidmark on the surface with collider
	///</summary>
	///<param name="pos">Position from where skidmarks must start</param>	
	///<param name="normal">Surface which is normal to the tyre</param>
	///<param name="intensity">Intensity of the skidmarks</param>
	///<param name="lastIndex">A variable that decides whether a skidmark must be drawn or not</param>
	public int AddSkidMark ( Vector3 pos ,   Vector3 normal ,   float intensity ,   int lastIndex  ){

		if(intensity > 1)
			intensity = 1.0f;
		if(intensity < 0)
			return -1;

		markSection curr = skidmarks[numMarks % maxMarks];
		curr.pos = pos + normal * groundOffset;
		curr.normal = normal;
		curr.intensity = intensity;
		curr.lastIndex = lastIndex;
		
		if(lastIndex != -1)
		{
			markSection last = skidmarks[lastIndex % maxMarks];
			Vector3 dir = (curr.pos - last.pos);
			Vector3 xDir = Vector3.Cross(dir,normal).normalized;
			
			curr.posl = curr.pos + xDir * markWidth * 0.5f;
			curr.posr = curr.pos - xDir * markWidth * 0.5f;
			curr.tangent = new Vector4(xDir.x, xDir.y, xDir.z, 1);
			
			if(last.lastIndex == -1)
			{
				last.tangent = curr.tangent;
				last.posl = curr.pos + xDir * markWidth * 0.5f;
				last.posr = curr.pos - xDir * markWidth * 0.5f;
			}
		}
		numMarks++;
		updated = true;
		return numMarks -1;

	}
	
	void  LateUpdate (){
		if(!updated)
		{
			return;
		}
		updated = false;
		
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.Clear();
		int segmentCount = 0;
		for(int j = 0; j < numMarks && j < maxMarks; j++)
			if(skidmarks[j].lastIndex != -1 && skidmarks[j].lastIndex > numMarks - maxMarks)
				segmentCount++;
		
		Vector3[] vertices = new Vector3[segmentCount * 4];
		Vector3[] normals = new Vector3[segmentCount * 4];
		Vector4[] tangents = new Vector4[segmentCount * 4];
		Color[] colors = new Color[segmentCount * 4];
		Vector2[] uvs = new Vector2[segmentCount * 4];
		int[] triangles = new int[segmentCount * 6];
		segmentCount = 0;
		for(int i = 0; i < numMarks && i < maxMarks; i++)
			if(skidmarks[i].lastIndex != -1 && skidmarks[i].lastIndex > numMarks - maxMarks)
		{
			markSection curr = skidmarks[i];
			markSection last = skidmarks[curr.lastIndex % maxMarks];
			vertices[segmentCount * 4 + 0] = last.posl;
			vertices[segmentCount * 4 + 1] = last.posr;
			vertices[segmentCount * 4 + 2] = curr.posl;
			vertices[segmentCount * 4 + 3] = curr.posr;
			
			normals[segmentCount * 4 + 0] = last.normal;
			normals[segmentCount * 4 + 1] = last.normal;
			normals[segmentCount * 4 + 2] = curr.normal;
			normals[segmentCount * 4 + 3] = curr.normal;
			
			tangents[segmentCount * 4 + 0] = last.tangent;
			tangents[segmentCount * 4 + 1] = last.tangent;
			tangents[segmentCount * 4 + 2] = curr.tangent;
			tangents[segmentCount * 4 + 3] = curr.tangent;
			
			colors[segmentCount * 4 + 0]=new Color(0, 0, 0, last.intensity);
			colors[segmentCount * 4 + 1]=new Color(0, 0, 0, last.intensity);
			colors[segmentCount * 4 + 2]=new Color(0, 0, 0, curr.intensity);
			colors[segmentCount * 4 + 3]=new Color(0, 0, 0, curr.intensity);
			
			uvs[segmentCount * 4 + 0] = new Vector2(0, 0);
			uvs[segmentCount * 4 + 1] = new Vector2(1, 0);
			uvs[segmentCount * 4 + 2] = new Vector2(0, 1);
			uvs[segmentCount * 4 + 3] = new Vector2(1, 1);
			
			triangles[segmentCount * 6 + 0] = segmentCount * 4 + 0;
			triangles[segmentCount * 6 + 2] = segmentCount * 4 + 1;
			triangles[segmentCount * 6 + 1] = segmentCount * 4 + 2;
			
			triangles[segmentCount * 6 + 3] = segmentCount * 4 + 2;
			triangles[segmentCount * 6 + 5] = segmentCount * 4 + 1;
			triangles[segmentCount * 6 + 4] = segmentCount * 4 + 3;
			segmentCount++;			
		}
		mesh.vertices=vertices;
		mesh.normals=normals;
		mesh.tangents=tangents;
		mesh.triangles=triangles;
		mesh.colors=colors;
		mesh.uv=uvs;
	}
}