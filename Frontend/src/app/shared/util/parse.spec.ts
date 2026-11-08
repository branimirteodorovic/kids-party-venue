import { toOptionalInt } from './parse';

describe('toOptionalInt', () => {
  it('should return undefined when the value is undefined', () => {
    expect(toOptionalInt(undefined)).toBeUndefined();
  });
  it('should return undefined when the value is empty string', () => {
    expect(toOptionalInt('')).toBeUndefined();
    expect(toOptionalInt(' ')).toBeUndefined();
  });
  it('should return undefined when the value is string', () => {
    expect(toOptionalInt('abc')).toBeUndefined();
  });
  it('should return undefined when the value is decimal number', () => {
    expect(toOptionalInt('1.5')).toBeUndefined();
  });

  it('should parse valid integer strings', () => {
    expect(toOptionalInt('15')).toBe(15);
    expect(toOptionalInt('-3')).toBe(-3);
  });
});
