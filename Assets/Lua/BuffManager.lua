-- BuffManager.lua
BuffManager = {}
BuffManager.ClassCache = {} -- 存储类原型（比如 buff2001 的类定义）

print("BuffManager 初始化")
function BuffManager.GetBuffInstance(buffId)
	local className = buffId
	local class = BuffManager.ClassCache[className]

	-- 1. 如果缓存没有，则 require 加载类定义
	if not class then
	-- 假设你的 buff 脚本最后 return 的是类本身，而不是 :new()
		class = require("Buff_" .. className)
		print(buffId)
		BuffManager.ClassCache[className] = class
	end

	-- 2. 返回一个新的实例
	if class and class.new then
		print("exist" .. buffId)
		return class:new()
	end

	error("Buff类加载失败: " .. className)
	return nil
end