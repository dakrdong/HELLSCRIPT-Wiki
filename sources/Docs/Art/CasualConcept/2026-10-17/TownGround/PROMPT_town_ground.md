# HELLSCRIPT town ground textures (casual StyleD)

Use the installed `imagegen` skill in built-in `image_gen` mode. Make ONE image_gen call per asset below (6 calls). After each call, copy the generated PNG byte-for-byte from `~/.codex/generated_images/...` into this working directory under the exact file name given. Do not edit, crop, resize, recolour, chroma-key or remove backgrounds yourself. Then write `manifest.json` listing for every file: file name, the exact prompt you sent, source path, pixel size, sha256, and (for the two alpha assets) alpha statistics (fraction of fully transparent pixels, fraction of fully opaque pixels). Record the model as "unknown" unless the tool reports it.

## Style reference (attached images)

The attached images are the approved concept art for this game's village: a bright, cheerful, hand-painted casual fantasy village seen from a high 3/4 top-down game camera. Match their palette and brushwork exactly: saturated sunny greens, warm cream and beige cobblestones, soft painterly shading, clean shapes, gentle highlights, no grit, no darkness, no realism. `mockup_overview.png` is the main reference; `crop_path_portal.png` shows the cobblestone path up close; `crop_grass_forest.png` shows the grass; `crop_training_yard.png` shows the sandy training yard.

## Assets

1. `town_grass_tile.png` — 1024x1024. A SEAMLESS TILEABLE ground texture of lush hand-painted meadow grass seen from directly above (pure orthographic top-down, no perspective, no horizon). Bright saturated spring greens like the reference (mid green base around #6cc04a with lighter yellow-green clumps around #9ad85a and a few deeper green strokes around #3f9a3a), soft painterly grass-clump shapes evenly spread across the whole square, a very sparse sprinkle of tiny white and pale-yellow flower dots. Even, flat, shadowless lighting. No paths, no stones, no objects, no single dominant feature, no vignette. Left edge must continue seamlessly into the right edge and top into bottom.

2. `town_cobble_tile.png` — 1024x1024. A SEAMLESS TILEABLE texture of a cobblestone village path seen from directly above (pure orthographic top-down). Rounded, slightly irregular cobbles in warm cream, beige, sand and pale tan (around #e9dcc0, #d8c6a2, #cdb68f) packed closely, with narrow soft warm-grey-beige gaps (around #b9a684) between them, gentle painterly shading on each stone, soft top light only. About 9 to 11 stones across the square. Clean and cheerful like the reference path, no moss, no cracks, no dirt, no grass, no puddles. Even lighting, no vignette, seamless on all edges.

3. `town_plaza_tile.png` — 1024x1024. A SEAMLESS TILEABLE texture of a town-square pavement seen from directly above (pure orthographic top-down). Larger, smooth, irregular flagstones (about 5 to 6 across the square) in warm cream, ivory and light sandstone (around #efe3c8, #e2d2b0, #d6c39c), a few slightly rosier or slightly greyer stones for variety, narrow soft beige gaps, gentle painterly shading. Same brushwork and palette as the cobblestone path in the reference. No moss, no cracks, no symbols. Even lighting, seamless on all edges.

4. `town_sand_tile.png` — 1024x1024. A SEAMLESS TILEABLE texture of a packed sandy-earth training ground seen from directly above (pure orthographic top-down). Warm light tan and sand (around #d9b47a, #cfa56a, #e3c28d) with soft painterly tonal patches, a few tiny pebbles and faint scuff strokes, cheerful and clean like the training yard in the reference. No footprints, no grass, no objects. Even lighting, seamless on all edges.

5. `town_grass_tuft.png` — 1024x1024 with NATIVE TRANSPARENT BACKGROUND (real alpha, not a painted checkerboard or white). One small clump of stylized hand-painted grass blades seen from the front, slightly from above, like the grass clumps in the reference: bright greens with lighter yellow-green tips, rounded friendly blade shapes, soft painterly shading, the clump's base flat along the bottom-centre of the image. The clump fills about 80% of the width. Nothing else in the image, no ground, no shadow.

6. `town_flower_clump.png` — 1024x1024 with NATIVE TRANSPARENT BACKGROUND (real alpha). One small cluster of hand-painted wildflowers like the reference: five to seven white daisies with yellow centres and two or three small yellow buttercups on short green stems among a few rounded leaves, seen from the front slightly from above, base flat along the bottom-centre. Bright, cheerful, soft painterly shading. Nothing else, no ground, no shadow.

When done, reply with the list of files and their sizes.
