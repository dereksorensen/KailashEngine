import bpy
import math


#######################################
## Export Settings
#######################################
export_selected = False


#######################################
## File Paths
#######################################

base_filename = bpy.path.basename(bpy.context.blend_data.filepath).replace(".blend","").replace(".blend","")

dae_filename = base_filename + '.dae'
dae_path = '..\\Mesh\\' + dae_filename

physics_filename = base_filename + '.physics'
physics_path = '..\\Physics\\' + physics_filename 

lights_filename = base_filename + '.lights'
lights_path = '..\\Lights\\' + lights_filename 


#######################################
## Export Collada File
#######################################
print("")
print("")

bpy.ops.wm.collada_export(
filepath=dae_path, 

selected=export_selected,
include_shapekeys=True,

apply_modifiers=True,
export_mesh_type_selection='view',

include_material_textures=True,
use_texture_copies=False,

triangulate=True,
use_object_instantiation=True,
sort_by_name=True,
)


#######################################
## Export Lights
#######################################
print("")
print("")
print("Lights Export: " + lights_path)


lights_file = open(lights_path, 'w')
numL = 0


for item in bpy.data.objects:
    print(item.name)
    if(item.type == 'LAMP'):
        if(bpy.data.lamps[item.name].type == 'SPOT' or bpy.data.lamps[item.name].type == 'POINT'):
            numL = numL + 1
        # border
            border = "== == ==" + '\n'
            lights_file.write(border)
        # name
            name = "nam " + item.name + '\n'
            lights_file.write(name)
        # type
            shape = "typ " + str(bpy.data.lamps[item.name].type) + '\n'
            lights_file.write(shape)
        #position
            position = "pos " + str(item.location.x) + " " + str(item.location.z) + " " + str(-item.location.y) + '\n'
            lights_file.write(position)
        #rotations
            rotation = "rot " + str(-item.rotation_euler.x) + " " + str(-item.rotation_euler.z) + " " + str(item.rotation_euler.y) + '\n'
            lights_file.write(rotation)
        #size
            size_float = (item.scale.x + item.scale.y + item.scale.z) / 3.0
            size = "siz " + str(size_float) + '\n'
            lights_file.write(size)
        #intensity
            intensity = "ity " + str(bpy.data.lamps[item.name].energy) + '\n'
            lights_file.write(intensity)
        #color
            color = "col " + str(bpy.data.lamps[item.name].color.r) + " " + str(bpy.data.lamps[item.name].color.g) + " " + str(bpy.data.lamps[item.name].color.b) + '\n'
            lights_file.write(color)
        #falloff
            falloff = "fal " + str(bpy.data.lamps[item.name].distance) + '\n'
            lights_file.write(falloff)
        #spot_angle
            spot_angle = "ang 0" + '\n'
            if (bpy.data.lamps[item.name].type == 'SPOT'):
                spot_angle = "ang " + str(bpy.data.lamps[item.name].spot_size) + '\n'                
            lights_file.write(spot_angle)
        #shadow
            shadow_method = str(bpy.data.lamps[item.name].shadow_method)
            has_shadow = 0
            if (not(shadow_method == 'NOSHADOW')):
                has_shadow = 1
            shadow = "sha " + str(has_shadow) + '\n'
            lights_file.write(shadow)
            
            lights_file.write('\n')

numL_string = "num " + str(numL)
lights_file.write(numL_string)

print("Info: Exported " + str(numL) + " Lights")


#######################################
## Export Physics
#######################################
print("")
print("")
print("Physics Export: " + physics_path)

physics_file = open(physics_path, 'w')
numRB = 0

for item in bpy.data.objects:
	if(item.type == 'MESH'):
		if(item.rigid_body and ((item.select and export_selected) or not(export_selected))):
			numRB = numRB + 1
		# border
			border = "== == ==" + '\n'
			physics_file.write(border)
		# name
			name = "nam " + item.name + '\n'
			physics_file.write(name)
        # mesh name
			mesh_name = "mam " + item.data.name + '\n'
			physics_file.write(mesh_name)
        # shape
			shape = "shp " + str(item.rigid_body.collision_shape) + '\n'
			physics_file.write(shape)
        # position
			position = "pos " + str(item.location.x) + " " + str(item.location.z) + " " + str(-item.location.y) + '\n'
			physics_file.write(position)
        #rotations
			rotation = "rot " + str(-item.rotation_euler.x) + " " + str(-item.rotation_euler.z) + " " + str(item.rotation_euler.y) + '\n'
			physics_file.write(rotation)
        # dimensions
			dimensions = "dim " + str(item.dimensions.x) + " " + str(item.dimensions.z) + " " + str(item.dimensions.y) + '\n'
			physics_file.write(dimensions)
        # scale
			scale = "scl " + str(item.scale.x) + " " + str(item.scale.z) + " " + str(item.scale.y) + '\n'
			physics_file.write(scale)
        # attributes
			attributes = "atr " + str(item.rigid_body.mass) + " " + str(item.rigid_body.friction) + " " + str(item.rigid_body.restitution) + '\n' 
			physics_file.write(attributes)
			
			physics_file.write('\n')
			
numRB_string = "num " + str(numRB)
physics_file.write(numRB_string)


print("Info: Exported " + str(numRB) + " Objects")
