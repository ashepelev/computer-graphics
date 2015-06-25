float4x4 World;

float4x4 WorldViewProj : WORLDVIEWPROJECTION; 

float4 vec_light; // ������� ��������� �����
float4 vec_view_pos; // ������ View
float4 vec_eye; // ������� �����������

float4 specular_color; // ���������� ����
float4 specular_intensity; // ������������� ����������� �����

struct PS_INPUT {
	float3 light : TEXCOORD0;
	float3 normal : TEXCOORD1;
	float3 view : TEXCOORD2;
};

struct VS_INPUT {
	float4 position : POSITION;
	float3 normal : NORMAL;
};

struct VS_OUTPUT {
	float4 position : POSITION;
	float3 light : TEXCOORD0; // ������� ��������� �����
	float3 normal : TEXCOORD1; // ������� � �����
	float3 view : TEXCOORD2; // ����������?
};


float4 PixelShaderFunction(PS_INPUT input) : COLOR {
	float power = 1;
	// input.normal - ��� ������� � ����� ���-������� ����������� � ������� �����������
	// input.view - ��� ���������� ���� ������� ������
	// ����� �� ������� ������ ���������� ���������� �������������� ������� �����������
	// (����� �� ������ "-", �.�. �� ������� �� �����) ������������ ������� � �����
	float3 reflect_vec = reflect(-normalize(input.view),input.normal);
//	float4 reflect_light = reflect(-normalize(vec_light),input.normal);
	// � ���������� ��������� ������ ����� - �� ������ �� ������, ����� ���������
	// ������ � ������ �������. ��� ����, ���� �� � ������� ����� �������� � �������� �����
	// ���������� ����, �� ��� "������" ������.
	// ����� �� ������� ��������� ������������ ����������� ������� ����������� � 
	// �������� ��������� �����. 
	float hotspot_effect = pow(dot(reflect_vec,input.light),power);
	
	float4 result = specular_color*specular_intensity*hotspot_effect;
	
	return result;
	
};

VS_OUTPUT VertexShaderFunction(VS_INPUT input) {		
	VS_OUTPUT result = (VS_OUTPUT) 0;
	result.position = mul(input.position,WorldViewProj);
	result.light = vec_light; 
	result.normal = normalize(mul(input.normal,WorldViewProj)); // normalize = x/length(x)
	// ����� ����������� ������� ����� ������� �������� � �������� ������� �����������
	// � ���������� �� �������� ������ V (�������)
	// �� ����� �������� �� ����� �� �����������
	// � ����� ������ �� ������ ��������� �� �����������
	result.view = vec_eye - vec_view_pos;
	return result;
}

technique Hotspot
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}

