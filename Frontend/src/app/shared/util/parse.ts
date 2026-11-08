export function toOptionalInt(value: string | undefined): number | undefined {
  const trimmedValue = value?.trim();
  if (!trimmedValue) {
    return undefined;
  }

  const num = Number(trimmedValue);
  if (!Number.isInteger(num)) {
    return undefined;
  }

  return num;
}
