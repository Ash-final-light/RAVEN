-- JumpGame.lua (这是新版本下载的脚本)
local JumpGame = {}

local playerPos = 0
local rootObj = nil

function JumpGame.Init(root)
	rootObj = root
	print("跳跃小游戏初始化成功！")
-- 这里可以根据需要实例化 AB 包里的 Prefab
end

function JumpGame.Update(dt)
-- 处理跳跃逻辑
	if UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space) then
		playerPos = playerPos + 5
		print("Lua 逻辑：玩家跳跃，高度：" .. playerPos)
	end
end

function JumpGame.OnDestroy()
	print("跳跃小游戏已卸载，释放资源")
	rootObj = nil
end

-- 关键：返回这个 Table 供 C# 映射
return JumpGame