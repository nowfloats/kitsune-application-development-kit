import * as fs from 'fs'

export const hasPlaceholders = async(localFilePath: string): Promise<boolean> => {
	let data = fs.readFileSync(localFilePath, 'utf-8')
		return /\[Kitsune:base64_.*\]/g.test(data) || /\[Kitsune_.*\]/g.test(data)
}