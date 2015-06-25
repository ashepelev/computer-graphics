	//float4 transformedNormal = mul(Normal, WorldViewProj);
	//Out.diff.rgba = GreenColor;
	//Out.diff *= dot(transformedNormal, LightDir);
	//Out.Position = mul(Pos,WorldViewProj);
	//return Out;
	
	
	//VertexShaderOutput output;

    //float4 worldPosition = mul(input.Position, World);
    //float4 viewPosition = mul(worldPosition, View);
    //output.Position = mul(viewPosition, Projection);

    //float4 normal = normalize(mul(input.Normal, WorldInverseTranspose));
    //float lightIntensity = dot(normal, DiffuseLightDirection);
    //output.Color = saturate(DiffuseColor * DiffuseIntensity * lightIntensity);

    //output.Normal = normal;

 //   return output;