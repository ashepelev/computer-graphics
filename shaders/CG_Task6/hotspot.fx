float4x4 World;

float4x4 WorldViewProj : WORLDVIEWPROJECTION; 

float4 vec_light; // ѕозици€ источника света
float4 vec_view_pos; // ¬ектор View
float4 vec_eye; // ѕозици€ наблюдател€

float4 specular_color; // отраженный свет
float4 specular_intensity; // интенсивность отраженного света

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
	float3 light : TEXCOORD0; // позици€ источника света
	float3 normal : TEXCOORD1; // нормаль к грани
	float3 view : TEXCOORD2; // –ассто€ние?
};


float4 PixelShaderFunction(PS_INPUT input) : COLOR {
	float power = 1;
	// input.normal - это нормаль к грани мэш-объекта приведенна€ к мировым координатам
	// input.view - это полученный нами видовой вектор
	// «десь мы находим вектор €вл€ющийс€ отражением нормированного вектора наблюдател€
	// (здесь со знаком "-", т.е. мы смотрим на грань) относительно нормали к грани
	float3 reflect_vec = reflect(-normalize(input.view),input.normal);
//	float4 reflect_light = reflect(-normalize(vec_light),input.normal);
	// ¬ результате достигает эффект блика - мы смотр€ на объект, видим отражени€
	// цветов в разные стороны. ѕри этом, если мы и истоник света образуем с нормалью грани
	// одинаковые углы, то нас "слепит" бликом.
	// «десь мы находим скал€рное произведение отраженного вектора наблюдател€ с 
	// ѕозицией источника света. 
	float hotspot_effect = pow(dot(reflect_vec,input.light),power);
	
	float4 result = specular_color*specular_intensity*hotspot_effect;
	
	return result;
	
};

VS_OUTPUT VertexShaderFunction(VS_INPUT input) {		
	VS_OUTPUT result = (VS_OUTPUT) 0;
	result.position = mul(input.position,WorldViewProj);
	result.light = vec_light; 
	result.normal = normalize(mul(input.normal,WorldViewProj)); // normalize = x/length(x)
	// «десь вычисл€етс€ разница между видовым вектором и вектором позиции наблюдател€
	// ¬ результате мы получаем вектор V (видовой)
	// ќн будет смотреть из грани на наблюдател€
	// ¬ нашем случай из центра координат на наблюдател€
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

