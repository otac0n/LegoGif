#local smooth_eb            = 0.50;
#local smooth_count         = 50;
#local final_pretrace_start = 64/image_width;
#local final_pretrace_end   = 4/image_width;
#local final_eb             = 0.1875;
#local final_count          = smooth_count * smooth_eb * smooth_eb / (final_eb * final_eb);

global_settings {
    ambient_light 0
    radiosity {
        pretrace_start final_pretrace_start
        pretrace_end final_pretrace_end
        count final_count
        nearest_count 20
        error_bound final_eb
        recursion_limit 4
        gray_threshold 0.5
        brightness 3
    }
}

sky_sphere {
    pigment {
        gradient y
        color_map {
            [0.0 color 1.0 * <1,1,1>]
            [0.3 color 0.5 * <1,1,1>]
            [1.0 color 0.1 * <1,1,1>]
        }
        scale 2
        translate -1
    }
}
