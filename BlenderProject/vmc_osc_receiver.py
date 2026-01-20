bl_info = {
    "name": "Virtual Motion Camera OSC Receiver",
    "author": "Antigravity",
    "version": (1, 0),
    "blender": (3, 6, 0),
    "location": "View3D > Sidebar > VMC",
    "description": "Receives camera tracking data via OSC",
    "category": "Camera",
}

import bpy
import socket
import struct
import threading

class VMC_Properties(bpy.types.PropertyGroup):
    port: bpy.props.IntProperty(name="Port", default=8000)
    is_running: bpy.props.BoolProperty(name="Is Running", default=False)
    camera_name: bpy.props.StringProperty(name="Camera", default="Camera")

class VMC_OT_StartReceiver(bpy.types.Operator):
    bl_idname = "vmc.start_receiver"
    bl_label = "Start OSC Receiver"
    
    _timer = None
    _socket = None
    _stop_event = None
    _data = [0.0] * 7 # x,y,z, rw,rx,ry,rz

    def modal(self, context, event):
        if event.type == 'TIMER':
            if not context.scene.vmc_props.is_running:
                self.stop_server(context)
                return {'FINISHED'}

            # Apply data to camera
            cam = bpy.data.objects.get(context.scene.vmc_props.camera_name)
            if cam:
                d = self._data
                # Unity(LHS) -> Blender(RHS)
                # Unity Z (Forward) -> Blender Y (Forward tested with 90deg offset)
                cam.location = (d[0], d[2], d[1])
                
                # Quat Sync
                import mathutils
                import math
                
                # Unity coordinates -> Blender coordinates conversion
                # Unity: x, y, z (Left Handed)
                # Blender: x, y, z (Right Handed)
                q = mathutils.Quaternion((d[3], d[4], d[5], d[6]))
                
                # Unity to Blender conversion (Switch LH to RH and axes)
                # This is a common mapping: Blender_x = Unity_x, Blender_y = -Unity_z, Blender_z = Unity_y
                # For orientation, a 90 deg rotation on X is often needed because Blender's cam looks down -Z but Unity's is +Z
                offset_rot = mathutils.Euler((math.radians(90), 0, 0)).to_quaternion()
                
                # Construct final rotation (Mapping might need adjustment based on behavior)
                # We apply the raw rotation then the 90 deg offset to "stand" the camera up
                cam.rotation_mode = 'QUATERNION'
                raw_q = mathutils.Quaternion((d[3], d[4], -d[6], d[5])) 
                cam.rotation_quaternion = raw_q @ offset_rot

        return {'PASS_THROUGH'}

    def execute(self, context):
        if context.scene.vmc_props.is_running:
            return {'CANCELLED'}
            
        context.scene.vmc_props.is_running = True
        self._stop_event = threading.Event()
        
        # Start UDP Thread
        port = context.scene.vmc_props.port
        threading.Thread(target=self.udp_server, args=(port,), daemon=True).start()
        
        wm = context.window_manager
        self._timer = wm.event_timer_add(0.01, window=context.window)
        wm.modal_handler_add(self)
        return {'RUNNING_MODAL'}

    def udp_server(self, port):
        sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        sock.bind(('', port))
        sock.settimeout(0.5)
        
        while not self._stop_event.is_set():
            try:
                data, addr = sock.recvfrom(1024)
                # Very simple parser for raw floats (assuming Unity sends 7 floats)
                # In a real OSC app, we would use an OSC library.
                if len(data) >= 28:
                    self._data = list(struct.unpack('!fffffff', data[:28]))
            except socket.timeout:
                continue
            except Exception as e:
                print(f"UDP Error: {e}")
                break
        sock.close()

    def stop_server(self, context):
        if self._stop_event:
            self._stop_event.set()
        wm = context.window_manager
        wm.event_timer_remove(self._timer)

class VMC_OT_StopReceiver(bpy.types.Operator):
    bl_idname = "vmc.stop_receiver"
    bl_label = "Stop OSC Receiver"
    
    def execute(self, context):
        context.scene.vmc_props.is_running = False
        return {'FINISHED'}

class VMC_PT_Panel(bpy.types.Panel):
    bl_label = "Virtual Motion Camera"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = 'VMC'

    def draw(self, context):
        layout = self.layout
        props = context.scene.vmc_props
        
        layout.prop(props, "camera_name")
        layout.prop(props, "port")
        
        if not props.is_running:
            layout.operator("vmc.start_receiver")
        else:
            layout.operator("vmc.stop_receiver")
            
        layout.separator()
        layout.label(text="Video Stream (NDI):")
        layout.operator("wm.url_open", text="Download Blender-NDI").url = "https://github.com/maybites/TextureSharing/releases"

def register():
    bpy.utils.register_class(VMC_Properties)
    bpy.types.Scene.vmc_props = bpy.props.PointerProperty(type=VMC_Properties)
    bpy.utils.register_class(VMC_OT_StartReceiver)
    bpy.utils.register_class(VMC_OT_StopReceiver)
    bpy.utils.register_class(VMC_PT_Panel)

def unregister():
    bpy.utils.unregister_class(VMC_PT_Panel)
    bpy.utils.unregister_class(VMC_OT_StopReceiver)
    bpy.utils.unregister_class(VMC_OT_StartReceiver)
    del bpy.types.Scene.vmc_props
    bpy.utils.unregister_class(VMC_Properties)

if __name__ == "__main__":
    register()
