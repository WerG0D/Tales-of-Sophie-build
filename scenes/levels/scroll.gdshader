shader_type canvas_item;
uniform vec2 camlocaloffset;

void fragment(){
	vec2 newuv = UV;
	newuv += camlocaloffset;
	vec4 c = texture(TEXTURE, newuv);
	COLOR = c;
}