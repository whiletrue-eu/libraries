sampler2D TextureSampler : register(s0);



/// <summary>Glow intensity</summary>
/// <minValue>0</minValue>
/// <maxValue>10</maxValue>
/// <defaultValue>2</defaultValue>
float Intensity : register(C0);

/// <summary>The radius of the Poisson disk (in pixels).</summary>
/// <minValue>1</minValue>
/// <maxValue>10</maxValue>
/// <defaultValue>5</defaultValue>
float Radius : register(C1);


/// <summary>Glow color</summary>
/// <minValue>#00000000</minValue>
/// <maxValue>#FFFFFFFF</maxValue>
/// <defaultValue>#FFFFFFFF</defaultValue>
float4 GlowColor: register(C2);

/// <defaultValue>0.01,0.01,0.01,0.01</defaultValue>
///<type>Point4D</type>
float4 Step : register(C3);

static const float2 poisson[152] = 
{
float2(-0.3587409, -0.4525569),
float2(-0.3470224, -0.6430249),
float2(-0.4776009, -0.5620176),
float2(-0.4215586, -0.3222934),
float2(-0.2410507, -0.5233843),
float2(-0.2146333, -0.3912685),
float2(-0.5356615, -0.37059),
float2(-0.2934854, -0.2573914),
float2(-0.5711179, -0.227865),
float2(-0.433771, -0.1706418),
float2(-0.07808571, -0.4201394),
float2(-0.08527222, -0.5722536),
float2(-0.035841, -0.2603325),
float2(-0.1857722, -0.1949348),
float2(-0.7090819, -0.3645873),
float2(-0.7423611, -0.2034348),
float2(-0.5293881, -0.08834367),
float2(-0.7317736, -0.05660628),
float2(0.06434289, -0.6066738),
float2(0.08650855, -0.4868758),
float2(0.05150771, -0.350668),
float2(-0.8060683, -0.464995),
float2(-0.6875291, -0.4922993),
float2(-0.8551547, -0.3254105),
float2(-0.02300608, -0.09679984),
float2(0.130175, -0.1192514),
float2(-0.1852314, -0.07328156),
float2(0.1327963, -0.253235),
float2(-0.1170398, -0.7096645),
float2(0.02038078, -0.7654446),
float2(-0.2363605, -0.6911869),
float2(-0.3453023, -0.7896044),
float2(-0.5084952, -0.7117317),
float2(-0.4505847, -0.8515052),
float2(0.3221376, -0.2195301),
float2(0.2709899, -0.07903628),
float2(0.25987, -0.4014685),
float2(0.005085011, 0.05172539),
float2(0.1196715, 0.009373903),
float2(0.24402, 0.0509523),
float2(0.4962199, -0.433384),
float2(0.4136061, -0.3410113),
float2(0.2219421, -0.5563302),
float2(0.3660916, -0.5263945),
float2(-0.3944232, 0.06587879),
float2(-0.5480451, 0.1233644),
float2(-0.3182935, -0.08689524),
float2(-0.7438884, -0.6335512),
float2(-0.6040959, -0.5903749),
float2(-0.2729416, 0.1079907),
float2(-0.1492528, 0.04317611),
float2(-0.2818317, -0.9196023),
float2(0.1523619, -0.6963552),
float2(0.3439711, -0.7093856),
float2(-0.1489136, -0.868324),
float2(-0.04559287, -0.9303055),
float2(0.6073328, -0.2840212),
float2(0.5376327, -0.1626445),
float2(0.6238101, -0.406898),
float2(0.548573, -0.5775815),
float2(0.4045914, -0.0175141),
float2(-0.4456474, 0.2353817),
float2(-0.2077861, 0.2089351),
float2(-0.9143829, -0.02622676),
float2(-0.8826719, -0.1493609),
float2(-0.8731478, 0.1070391),
float2(-0.6618989, 0.0626179),
float2(-0.6970007, 0.1777206),
float2(-0.6436763, -0.7552376),
float2(0.3791283, -0.8320122),
float2(0.2414675, -0.8737681),
float2(0.4726676, -0.7234905),
float2(-0.002110025, 0.2547172),
float2(0.1124841, 0.1415709),
float2(-0.6460828, 0.2997737),
float2(-0.7945891, 0.2513569),
float2(0.7411628, -0.5245491),
float2(0.8086323, -0.3211663),
float2(0.8564064, -0.4505535),
float2(0.6788611, -0.1875789),
float2(0.6629376, -0.6255594),
float2(0.2829114, 0.2248589),
float2(0.1550976, 0.3138398),
float2(0.04939627, 0.3763308),
float2(0.2286339, 0.5420712),
float2(0.3623347, 0.4002457),
float2(0.1023397, 0.5065688),
float2(-0.7504656, 0.4102657),
float2(-0.6414021, 0.486967),
float2(-0.5184858, 0.3992499),
float2(0.8071183, -0.1856098),
float2(-0.07476149, 0.4210323),
float2(-0.07519461, 0.1517977),
float2(-0.1839517, 0.3400778),
float2(-0.3040845, 0.5133319),
float2(-0.1781421, 0.5318108),
float2(-0.3438758, 0.3760517),
float2(-0.389396, 0.5978833),
float2(-0.5569223, 0.5914961),
float2(0.08338258, -0.996498),
float2(0.7178547, -0.06557118),
float2(0.8553843, -0.04934356),
float2(0.6074598, 0.02577501),
float2(0.1028431, -0.878042),
float2(-0.2700751, 0.7525558),
float2(-0.03114604, 0.5908657),
float2(-0.1122682, 0.7348565),
float2(-0.8403122, 0.5156046),
float2(-0.7277512, 0.6257173),
float2(-0.5824915, 0.7386944),
float2(-0.4547664, 0.7331639),
float2(0.597621, -0.7877226),
float2(0.9268396, -0.2368845),
float2(0.9745665, -0.09617025),
float2(0.5202205, 0.1100013),
float2(0.3821025, 0.1234652),
float2(0.4177925, -0.1432205),
float2(0.4532907, 0.2860522),
float2(0.5742688, 0.4035887),
float2(0.4052311, 0.6167075),
float2(0.4920275, 0.52144),
float2(-0.3345222, 0.8712034),
float2(-0.08406424, 0.898145),
float2(-0.2071881, 0.8784452),
float2(-0.9143628, 0.3262814),
float2(-0.9533357, 0.2059974),
float2(-0.4565315, 0.8860006),
float2(-0.2696952, 0.6316104),
float2(0.05174352, 0.7497503),
float2(0.1369462, 0.8983034),
float2(0.0347296, 0.9874865),
float2(0.2671542, 0.7126285),
float2(0.1411953, 0.6310524),
float2(0.2400754, 0.4067924),
float2(0.7837553, 0.04730136),
float2(0.6360359, 0.2133837),
float2(0.605151, 0.7128388),
float2(0.4618908, 0.8450137),
float2(0.6416615, 0.5913453),
float2(0.683133, 0.3325755),
float2(0.7101392, 0.491923),
float2(0.9606325, 0.03911939),
float2(0.9347865, 0.198305),
float2(0.2593718, 0.8780994),
float2(0.7866962, 0.5966813),
float2(-0.3231773, 0.250861),
float2(0.820079, 0.4098022),
float2(-0.4327188, 0.4844651),
float2(0.7954795, 0.209889),
float2(0.914568, 0.320159),
float2(-0.9616365, -0.2457018),
float2(0.4778781, 0.716781)
};

float4 main(float2 Coordinate : TEXCOORD0) : COLOR  
{
	// Take samples accoriding to poisson table above.
	// We need a high number of samples as this effect is used most probably with text which has only some
	// thin lines. having too less samples means that we dont get a smooth blur.
	float2 InputSize= 1.0/Step.xw; // x and w contains the fragment of 1 which is the step from one pixel to the next
	float Opacity=0;
	for(int Tap = 0; Tap < 152; Tap++)
    {
        Opacity += tex2D(TextureSampler, Coordinate.xy + (poisson[Tap] / InputSize * Radius)).a;
    }
    Opacity /= 152;
    
    // Generate Glow color. 
    // Glow is generated from the opacity sampled above, an intesity factor to boost
    // the opacity, limited by the opacity of the glow color, and mixed with the color of the Glow color
    float4 Glow;
	Glow.rgb=GlowColor.rgb*min(Intensity*Opacity,GlowColor.a);
	Glow.a=min(Opacity*Intensity,GlowColor.a);

	//Get the original color value for the current coordinate
    float4 Color = tex2D(TextureSampler, Coordinate);
    
    // Mix the Glow behind the original color
    // Mixing is done by taking the 'leftover' opacity of the original
    float4 Result;
    Result.a = Color.a+Glow.a;
    Result.rgb = Color.rgb+Glow.rgb*(1-Color.a);

    return Result;
}  
